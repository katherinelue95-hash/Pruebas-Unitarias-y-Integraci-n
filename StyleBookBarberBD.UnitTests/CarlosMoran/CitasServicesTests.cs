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

public class CitasServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly CitasServices servicio;
    private readonly DateTime fecha = new(2030, 6, 10, 10, 0, 0);

    public CitasServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new StyleBookBarberBDContext(opciones);

        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);

        servicio = new CitasServices(db, configuracion.CreateMapper());

        db.Usuarios.AddRange(
            new Usuarios {
                UsuariosId = 1, Nombre = "Cliente", Apellido = "Prueba",
                Correo = "cliente@example.com", PasswordHash = "prueba"
            },
            new Usuarios {
                UsuariosId = 2, Nombre = "Barbero", Apellido = "Prueba",
                Correo = "barbero@example.com", PasswordHash = "prueba"
            });

        db.Barberos.Add(new Barberos {
            BarberosId = 1, UsuariosId = 2, Especialidad = "Cortes"
        });

        db.Servicios.Add(new Servicios {
            ServiciosId = 1, NombreServicio = "Corte",
            Precio = 5m, DuracionMin = 30
        });

        db.SaveChanges();
    }

    private CitasDTos NuevaCita() => new()
    {
        ClienteId = 1,
        BarberosId = 1,
        ServiciosId = 1,
        FechaHora = fecha,
        MontoTotal = 5m
    };

    [Fact]
    public async Task CrearCita_Valida_GuardaConEstadoPendiente()
    {
        var solicitud = NuevaCita();
        solicitud.Estado = "Completada";

        var resultado = await servicio.CrearCitaAsync(solicitud);

        Assert.NotNull(resultado);
        Assert.True(resultado.CitasId > 0);
        Assert.Equal("Pendiente", resultado.Estado);
        Assert.Equal("Cliente Prueba", resultado.ClienteNombre);
        Assert.Equal("Corte", resultado.ServicioNombre);

        var guardada = Assert.Single(await db.Citas.ToListAsync());
        Assert.Equal(fecha, guardada.FechaHora);
        Assert.Equal(5m, guardada.MontoTotal);
    }

    [Theory]
    [InlineData(999, 1, 1)]
    [InlineData(1, 999, 1)]
    [InlineData(1, 1, 999)]
    public async Task CrearCita_ReferenciaInexistente_NoGuarda(
        int cliente, int barbero, int servicioId)
    {
        var solicitud = NuevaCita();
        solicitud.ClienteId = cliente;
        solicitud.BarberosId = barbero;
        solicitud.ServiciosId = servicioId;

        var resultado = await servicio.CrearCitaAsync(solicitud);

        Assert.Null(resultado);
        Assert.Empty(await db.Citas.ToListAsync());
    }

    [Fact]
    public async Task CrearCita_MismoBarberoYHora_RechazaDuplicado()
    {
        Assert.NotNull(await servicio.CrearCitaAsync(NuevaCita()));

        var resultado = await servicio.CrearCitaAsync(NuevaCita());

        Assert.Null(resultado);
        Assert.Single(await db.Citas.ToListAsync());
    }

    [Fact]
    public async Task CrearCita_HoraDiferente_PermiteReserva()
    {
        Assert.NotNull(await servicio.CrearCitaAsync(NuevaCita()));
        var otra = NuevaCita();
        otra.FechaHora = fecha.AddHours(1);

        var resultado = await servicio.CrearCitaAsync(otra);

        Assert.NotNull(resultado);
        Assert.Equal(2, await db.Citas.CountAsync());
    }

    [Theory]
    [InlineData("Pendiente")]
    [InlineData("Confirmada")]
    [InlineData("En curso")]
    [InlineData("Completada")]
    [InlineData("Cancelada")]
    public async Task CambiarEstado_ValorPermitido_PersisteCambio(string estado)
    {
        var creada = await servicio.CrearCitaAsync(NuevaCita());
        Assert.NotNull(creada);

        var resultado = await servicio.CambiarEstadoAsync(creada.CitasId, estado);

        Assert.NotNull(resultado);
        Assert.Equal(estado, resultado.Estado);
        Assert.Equal(estado, (await db.Citas.FindAsync(creada.CitasId))!.Estado);
    }

    [Fact]
    public async Task CambiarEstado_ValorInvalido_ConservaEstadoOriginal()
    {
        var creada = await servicio.CrearCitaAsync(NuevaCita());
        Assert.NotNull(creada);

        var resultado = await servicio.CambiarEstadoAsync(creada.CitasId, "Invalido");

        Assert.Null(resultado);
        Assert.Equal("Pendiente", (await db.Citas.FindAsync(creada.CitasId))!.Estado);
    }

    [Fact]
    public async Task CambiarEstado_CitaInexistente_RetornaNull()
    {
        Assert.Null(await servicio.CambiarEstadoAsync(999, "Confirmada"));
    }

    [Fact]
    public async Task Consultar_CitaInexistente_RetornaNull()
    {
        Assert.Null(await servicio.GetCitaByIdAsync(999));
    }

    [Fact]
    public async Task Eliminar_CitaExistente_QuitaRegistro()
    {
        var creada = await servicio.CrearCitaAsync(NuevaCita());
        Assert.NotNull(creada);

        Assert.True(await servicio.EliminarCitaAsync(creada.CitasId));
        Assert.Null(await db.Citas.FindAsync(creada.CitasId));
    }

    [Fact]
    public async Task Eliminar_CitaInexistente_RetornaFalse()
    {
        Assert.False(await servicio.EliminarCitaAsync(999));
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
