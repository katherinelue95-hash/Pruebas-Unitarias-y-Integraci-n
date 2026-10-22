using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests;

public class ResenasServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly ResenasServices servicio;

    public ResenasServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        db = new StyleBookBarberBDContext(opciones);
        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);
        servicio = new ResenasServices(db, configuracion.CreateMapper());

        db.Usuarios.AddRange(
            new Usuarios { UsuariosId = 1, Nombre = "Marta", Apellido = "Lopez", Correo = "marta@example.com", PasswordHash = "test" },
            new Usuarios { UsuariosId = 2, Nombre = "Juan", Apellido = "Perez", Correo = "juan@example.com", PasswordHash = "test" },
            new Usuarios { UsuariosId = 3, Nombre = "Rosa", Apellido = "Diaz", Correo = "rosa@example.com", PasswordHash = "test" });
        db.Barberos.AddRange(
            new Barberos { BarberosId = 1, UsuariosId = 1, Especialidad = "Cortes", Calificacion = 0m },
            new Barberos { BarberosId = 2, UsuariosId = 3, Especialidad = "Barba", Calificacion = 0m });
        db.SaveChanges();
    }

    private static ResenasDTos NuevaResena(int estrellas = 5, int barberoId = 1, int usuarioId = 2)
        => new()
        {
            BarberosId = barberoId, UsuariosId = usuarioId,
            Estrellas = estrellas, Comentario = "Muy buen servicio"
        };

    private void Guardar(int id, int barberoId, int estrellas, DateTime? fecha = null)
    {
        db.Resenas.Add(new Resenas
        {
            ResenaId = id, BarberosId = barberoId, UsuariosId = 2,
            Estrellas = estrellas, Comentario = "Resena guardada",
            Fecha = fecha ?? new DateTime(2030, 1, 1)
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task Listar_SinResenas_RetornaVacio()
    {
        Assert.Empty(await servicio.GetResenasAsync());
    }

    [Fact]
    public async Task Listar_ConResenas_OrdenaYDevuelveAutorBarbero()
    {
        Guardar(10, 1, 3, new DateTime(2030, 1, 1));
        Guardar(20, 1, 5, new DateTime(2030, 1, 3));
        var barbero = (await db.Barberos.FindAsync(1))!;
        barbero.Calificacion = 4m;
        await db.SaveChangesAsync();

        var lista = await servicio.GetResenasAsync();
        Assert.Equal(2, lista.Count);
        Assert.Equal(20, lista[0].ResenaId);
        Assert.Equal("Juan Perez", lista[0].Autor);
        Assert.Equal("Marta Lopez", lista[0].BarberoNombre);
        Assert.Equal(4m, lista[0].CalificacionPromedio);
    }

    [Fact]
    public async Task PorBarbero_FiltraCorrectamente()
    {
        Guardar(10, 1, 5);
        Guardar(20, 1, 4);
        Guardar(30, 2, 1);
        (await db.Barberos.FindAsync(1))!.Calificacion = 4.5m;
        await db.SaveChangesAsync();

        var lista = await servicio.GetResenasByBarberoAsync(1);
        Assert.Equal(2, lista.Count);
        Assert.All(lista, x => Assert.Equal(1, x.BarberosId));
        Assert.All(lista, x => Assert.Equal(4.5m, x.CalificacionPromedio));
    }

    [Fact]
    public async Task PorBarbero_Inexistente_RetornaVacio()
    {
        Assert.Empty(await servicio.GetResenasByBarberoAsync(999));
    }

    [Fact]
    public async Task Crear_Valida_GuardaYActualizaPromedio()
    {
        var creada = await servicio.CrearResenaAsync(NuevaResena(5));
        Assert.NotNull(creada);
        Assert.True(creada.ResenaId > 0);
        Assert.Equal(5m, creada.CalificacionPromedio);
        Assert.Equal("Juan Perez", creada.Autor);
        Assert.Equal(1, await db.Resenas.CountAsync());
        Assert.Equal(5m, (await db.Barberos.FindAsync(1))!.Calificacion);
        Assert.Equal(3, await db.Usuarios.CountAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Crear_EstrellasFueraDeRango_Rechaza(int estrellas)
    {
        Assert.Null(await servicio.CrearResenaAsync(NuevaResena(estrellas)));
        Assert.Empty(await db.Resenas.ToListAsync());
    }

    [Fact]
    public async Task Crear_UsuarioInexistente_NoGuarda()
    {
        Assert.Null(await servicio.CrearResenaAsync(NuevaResena(usuarioId: 999)));
        Assert.Equal(0, await db.Resenas.CountAsync());
    }

    [Fact]
    public async Task Crear_BarberoInexistente_NoGuarda()
    {
        Assert.Null(await servicio.CrearResenaAsync(NuevaResena(barberoId: 999)));
        Assert.Equal(0, await db.Resenas.CountAsync());
    }

    [Fact]
    public async Task Crear_SegundaResena_RecalculaMedia()
    {
        var primera = await servicio.CrearResenaAsync(NuevaResena(5));
        var segunda = await servicio.CrearResenaAsync(NuevaResena(3));
        Assert.NotNull(primera);
        Assert.NotNull(segunda);
        Assert.Equal(4m, segunda.CalificacionPromedio);
        Assert.Equal(4m, (await db.Barberos.FindAsync(1))!.Calificacion);
        Assert.Equal(2, await db.Resenas.CountAsync());
    }

    [Fact]
    public async Task Crear_ResenasDeBarberosDistintos_NoMezclaPromedios()
    {
        Assert.NotNull(await servicio.CrearResenaAsync(NuevaResena(5, 1)));
        Assert.NotNull(await servicio.CrearResenaAsync(NuevaResena(2, 2)));
        Assert.Equal(5m, (await db.Barberos.FindAsync(1))!.Calificacion);
        Assert.Equal(2m, (await db.Barberos.FindAsync(2))!.Calificacion);
    }

    [Fact]
    public async Task Eliminar_UnicaResena_RestablecePromedioACero()
    {
        var creada = await servicio.CrearResenaAsync(NuevaResena(5));
        Assert.NotNull(creada);
        Assert.True(await servicio.EliminarResenaAsync(creada.ResenaId));
        Assert.Empty(await db.Resenas.ToListAsync());
        Assert.Equal(0m, (await db.Barberos.FindAsync(1))!.Calificacion);
    }

    [Fact]
    public async Task Eliminar_ConOtraResena_RecalculaPromedio()
    {
        var primera = await servicio.CrearResenaAsync(NuevaResena(5));
        var segunda = await servicio.CrearResenaAsync(NuevaResena(3));
        Assert.NotNull(primera);
        Assert.NotNull(segunda);
        Assert.True(await servicio.EliminarResenaAsync(segunda.ResenaId));
        Assert.Equal(5m, (await db.Barberos.FindAsync(1))!.Calificacion);
        Assert.Equal(1, await db.Resenas.CountAsync());
    }

    [Fact]
    public async Task Eliminar_Inexistente_RetornaFalse()
    {
        Assert.False(await servicio.EliminarResenaAsync(999));
    }

    [Fact]
    public async Task Eliminar_SinBarberoAsociado_NoFalla()
    {
        // El proveedor InMemory permite guardar un registro huérfano;
        // este caso evalúa la rama defensiva del servicio.
        Guardar(10, 999, 3);
        Assert.True(await servicio.EliminarResenaAsync(10));
        Assert.Equal(0, await db.Resenas.CountAsync());
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
