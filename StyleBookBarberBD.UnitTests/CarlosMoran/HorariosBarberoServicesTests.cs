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

public class HorariosBarberoServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly HorariosBarberoServices servicio;

    public HorariosBarberoServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        db = new StyleBookBarberBDContext(opciones);
        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);
        servicio = new HorariosBarberoServices(db, configuracion.CreateMapper());

        db.Usuarios.AddRange(
            new Usuarios { UsuariosId = 1, Nombre = "Ana", Apellido = "Ruiz", Correo = "ana@example.com", PasswordHash = "test" },
            new Usuarios { UsuariosId = 2, Nombre = "Leo", Apellido = "Lopez", Correo = "leo@example.com", PasswordHash = "test" });
        db.Barberos.AddRange(
            new Barberos { BarberosId = 1, UsuariosId = 1, Especialidad = "Cortes" },
            new Barberos { BarberosId = 2, UsuariosId = 2, Especialidad = "Barba" });
        db.SaveChanges();
    }

    private HorariosBarbero GuardarHorario(int id, int barberoId = 1)
    {
        var horario = new HorariosBarbero
        {
            HorariosId = id, BarberosId = barberoId, DiaSemana = 1,
            HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(17, 0, 0)
        };
        db.HorariosBarbero.Add(horario);
        db.SaveChanges();
        return horario;
    }

    private static HorariosBarberoDTos NuevoHorario(int barberoId = 1) => new()
    {
        BarberoId = barberoId, DiaSemana = 2, HoraInicio = "09:00", HoraFin = "18:30"
    };

    [Fact]
    public async Task Listar_SinHorarios_RetornaVacio()
    {
        Assert.Empty(await servicio.GetHorariosAsync());
    }

    [Fact]
    public async Task Listar_ConHorarios_RetornaTodosYConvierteHoras()
    {
        GuardarHorario(10);
        GuardarHorario(20, 2);
        var lista = (await servicio.GetHorariosAsync()).ToList();
        Assert.Equal(2, lista.Count);
        Assert.Contains(lista, x => x.HorariosId == 10 && x.BarberoId == 1 && x.HoraInicio == "08:00");
        Assert.Contains(lista, x => x.HorariosId == 20 && x.BarberoId == 2 && x.HoraFin == "17:00");
    }

    [Fact]
    public async Task Consultar_Existente_RetornaDatos()
    {
        GuardarHorario(10);
        var resultado = await servicio.GetHorarioByIdAsync(10);
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.HorariosId);
        Assert.Equal(1, resultado.BarberoId);
        Assert.Equal("08:00", resultado.HoraInicio);
        Assert.Equal("17:00", resultado.HoraFin);
    }

    [Fact]
    public async Task Consultar_Inexistente_RetornaNull()
    {
        Assert.Null(await servicio.GetHorarioByIdAsync(999));
    }

    [Fact]
    public async Task Crear_HorarioValido_GuardaLlaveYHoras()
    {
        var creado = await servicio.CrearHorarioAsync(NuevoHorario());
        Assert.True(creado.HorariosId > 0);
        Assert.Equal(1, creado.BarberoId);
        Assert.Equal("09:00", creado.HoraInicio);
        Assert.Equal("18:30", creado.HoraFin);
        var guardado = await db.HorariosBarbero.FindAsync(creado.HorariosId);
        Assert.NotNull(guardado);
        Assert.Equal(1, guardado.BarberosId);
        Assert.Equal(new TimeSpan(9, 0, 0), guardado.HoraInicio);
        Assert.Equal(new TimeSpan(18, 30, 0), guardado.HoraFin);
    }

    [Fact]
    public async Task Crear_DosHorarios_GuardaAmbos()
    {
        var primero = await servicio.CrearHorarioAsync(NuevoHorario());
        var segundo = await servicio.CrearHorarioAsync(NuevoHorario(2));
        Assert.NotEqual(primero.HorariosId, segundo.HorariosId);
        Assert.Equal(2, await db.HorariosBarbero.CountAsync());
    }

    [Fact]
    public async Task Actualizar_Existente_ModificaDatos()
    {
        GuardarHorario(10);
        var dto = new HorariosBarberoDTos
        {
            HorariosId = 10, BarberoId = 2, DiaSemana = 5,
            HoraInicio = "10:15", HoraFin = "19:45"
        };
        var actualizado = await servicio.ActualizarHorarioAsync(10, dto);
        Assert.NotNull(actualizado);
        Assert.Equal(10, actualizado.HorariosId);
        Assert.Equal(2, actualizado.BarberoId);
        Assert.Equal("10:15", actualizado.HoraInicio);
        var guardado = await db.HorariosBarbero.FindAsync(10);
        Assert.NotNull(guardado);
        Assert.Equal(2, guardado.BarberosId);
        Assert.Equal(5, guardado.DiaSemana);
        Assert.Equal(new TimeSpan(19, 45, 0), guardado.HoraFin);
    }

    [Fact]
    public async Task Actualizar_Inexistente_RetornaNull()
    {
        Assert.Null(await servicio.ActualizarHorarioAsync(999, NuevoHorario()));
    }

    [Fact]
    public async Task Eliminar_Existente_BorraHorario()
    {
        GuardarHorario(10);
        Assert.True(await servicio.EliminarHorarioAsync(10));
        Assert.Null(await db.HorariosBarbero.FindAsync(10));
    }

    [Fact]
    public async Task Eliminar_Inexistente_RetornaFalse()
    {
        Assert.False(await servicio.EliminarHorarioAsync(999));
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
