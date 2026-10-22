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

public class BarberosServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly BarberosServices servicio;

    public BarberosServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new StyleBookBarberBDContext(opciones);

        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);

        servicio = new BarberosServices(
            db, configuracion.CreateMapper());

        db.Usuarios.AddRange(
            new Usuarios
            {
                UsuariosId = 1,
                Nombre = "Carlos",
                Apellido = "Moran",
                Correo = "carlos@example.com",
                PasswordHash = "prueba"
            },
            new Usuarios
            {
                UsuariosId = 2,
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                PasswordHash = "prueba"
            });

        db.SaveChanges();
    }

    private Barberos CrearRegistro(int id, int usuarioId)
    {
        var barbero = new Barberos
        {
            BarberosId = id,
            UsuariosId = usuarioId,
            Especialidad = "Cortes clasicos",
            FotoUrl = "foto.jpg",
            Calificacion = 4.5m,
            EstadoDisp = "Disponible"
        };

        db.Barberos.Add(barbero);
        db.SaveChanges();

        return barbero;
    }

    private BarberosDTos NuevoBarbero() => new()
    {
        UsuarioId = 1,
        Especialidad = "Degradados",
        FotoUrl = "barbero.jpg",
        Calificacion = 4.8m,
        EstadoDisp = "Disponible"
    };

    [Fact]
    public async Task ConsultarBarberos_SinRegistros_RetornaListaVacia()
    {
        var resultado = await servicio.GetBarberosAsync();

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ConsultarBarberos_ConRegistros_RetornaTodos()
    {
        CrearRegistro(10, 1);
        CrearRegistro(20, 2);

        var resultado = await servicio.GetBarberosAsync();

        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, b => b.BarberosId == 10);
        Assert.Contains(resultado, b => b.BarberosId == 20);
    }

    [Fact]
    public async Task ConsultarPorId_Existente_RetornaBarbero()
    {
        CrearRegistro(10, 1);

        var resultado = await servicio.GetBarberoByIdAsync(10);

        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.BarberosId);
        Assert.Equal("Cortes clasicos", resultado.Especialidad);
    }

    [Fact]
    public async Task ConsultarPorId_Inexistente_RetornaNull()
    {
        var resultado = await servicio.GetBarberoByIdAsync(999);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task CrearBarbero_Valido_GuardaRegistro()
    {
        var dto = NuevoBarbero();

        var resultado = await servicio.CrearBarberoAsync(dto);

        Assert.NotNull(resultado);
        Assert.True(resultado.BarberosId > 0);

        var guardado = await db.Barberos
            .FindAsync(resultado.BarberosId);

        Assert.NotNull(guardado);
        Assert.Equal(1, guardado.UsuariosId);
        Assert.Equal("Degradados", guardado.Especialidad);
        Assert.Equal(4.8m, guardado.Calificacion);
    }

    [Fact]
    public async Task CrearDosBarberos_GuardaAmbosRegistros()
    {
        var primero = NuevoBarbero();

        var segundo = NuevoBarbero();
        segundo.UsuarioId = 2;
        segundo.Especialidad = "Barba";

        var resultado1 = await servicio.CrearBarberoAsync(primero);
        var resultado2 = await servicio.CrearBarberoAsync(segundo);

        Assert.NotEqual(
            resultado1.BarberosId,
            resultado2.BarberosId);

        Assert.Equal(2, await db.Barberos.CountAsync());
    }

    [Fact]
    public async Task ActualizarBarbero_Existente_GuardaCambios()
    {
        CrearRegistro(10, 1);

        var dto = new BarberosDTos
        {
            UsuarioId = 2,
            Especialidad = "Colorimetria",
            FotoUrl = "nueva-foto.jpg",
            Calificacion = 4.9m,
            EstadoDisp = "Ocupado"
        };

        var resultado = await servicio
            .ActualizarBarberoAsync(10, dto);

        Assert.NotNull(resultado);

        var guardado = await db.Barberos.FindAsync(10);

        Assert.NotNull(guardado);
        Assert.Equal(10, guardado.BarberosId);
        Assert.Equal(2, guardado.UsuariosId);
        Assert.Equal("Colorimetria", guardado.Especialidad);
        Assert.Equal("nueva-foto.jpg", guardado.FotoUrl);
        Assert.Equal(4.9m, guardado.Calificacion);
        Assert.Equal("Ocupado", guardado.EstadoDisp);
    }

    [Fact]
    public async Task ActualizarBarbero_Inexistente_RetornaNull()
    {
        var dto = NuevoBarbero();

        var resultado = await servicio
            .ActualizarBarberoAsync(999, dto);

        Assert.Null(resultado);
        Assert.Empty(await db.Barberos.ToListAsync());
    }

    [Fact]
    public async Task EliminarBarbero_Existente_EliminaRegistro()
    {
        CrearRegistro(10, 1);

        var resultado = await servicio.EliminarBarberoAsync(10);

        Assert.True(resultado);
        Assert.Null(await db.Barberos.FindAsync(10));
        Assert.Empty(await db.Barberos.ToListAsync());
    }

    [Fact]
    public async Task EliminarBarbero_Inexistente_RetornaFalse()
    {
        var resultado = await servicio.EliminarBarberoAsync(999);

        Assert.False(resultado);
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
