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

public class ServiciosServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly ServiciosServices servicio;

    public ServiciosServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new StyleBookBarberBDContext(opciones);

        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);
        servicio = new ServiciosServices(db, configuracion.CreateMapper());

        db.Categorias.Add(new Categorias
        {
            CategoriasId = 1,
            NombreCategoria = "Cortes",
            Descripcion = "Cortes de cabello"
        });
        db.Servicios.Add(new Servicios
        {
            ServiciosId = 1,
            CategoriaId = 1,
            NombreServicio = "Corte clasico",
            Precio = 5m,
            DuracionMin = 30
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task Listar_IncluyeServicioYCategoria()
    {
        var resultado = await servicio.GetServiciosAsync();

        var item = Assert.Single(resultado);
        Assert.Equal(1, item.ServiciosId);
        Assert.Equal("Corte clasico", item.NombreServicio);
        Assert.Equal("Cortes", item.NombreCategoria);
        Assert.Equal(5m, item.Precio);
    }

    [Fact]
    public async Task Consultar_Existente_DevuelveDatosCorrectos()
    {
        var resultado = await servicio.GetServicioByIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal("Corte clasico", resultado.NombreServicio);
        Assert.Equal(30, resultado.DuracionMin);
        Assert.Equal("Cortes", resultado.NombreCategoria);
    }

    [Fact]
    public async Task Consultar_Inexistente_DevuelveNull()
    {
        Assert.Null(await servicio.GetServicioByIdAsync(999));
    }

    [Fact]
    public async Task Crear_GuardaPrecioDuracionYCategoria()
    {
        var resultado = await servicio.CrearServicioAsync(new ServiciosDTos
        {
            CategoriaId = 1,
            NombreCategoria = "Cortes",
            NombreServicio = "Corte premium",
            Precio = 12.50m,
            DuracionMin = 45,
            FotoUrl = "/uploads/prueba.jpg"
        });

        Assert.True(resultado.ServiciosId > 1);
        db.ChangeTracker.Clear();

        var guardado = await db.Servicios.FindAsync(resultado.ServiciosId);
        Assert.NotNull(guardado);
        Assert.Equal("Corte premium", guardado.NombreServicio);
        Assert.Equal(12.50m, guardado.Precio);
        Assert.Equal(45, guardado.DuracionMin);
        Assert.Equal(1, guardado.CategoriaId);
        Assert.Equal("/uploads/prueba.jpg", guardado.FotoUrl);
    }

    [Fact]
    public async Task Actualizar_Existente_PersisteCambios()
    {
        var resultado = await servicio.ActualizarServicioAsync(1, new ServiciosDTos
        {
            CategoriaId = 1,
            NombreServicio = "Corte actualizado",
            Precio = 8m,
            DuracionMin = 40,
            FotoUrl = "/uploads/nueva.jpg"
        });

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.ServiciosId);
        db.ChangeTracker.Clear();

        var guardado = await db.Servicios.FindAsync(1);
        Assert.NotNull(guardado);
        Assert.Equal("Corte actualizado", guardado.NombreServicio);
        Assert.Equal(8m, guardado.Precio);
        Assert.Equal(40, guardado.DuracionMin);
        Assert.Equal("/uploads/nueva.jpg", guardado.FotoUrl);
        Assert.Equal(1, await db.Servicios.CountAsync());
    }

    [Fact]
    public async Task Actualizar_Inexistente_NoCreaRegistro()
    {
        var resultado = await servicio.ActualizarServicioAsync(
            999, new ServiciosDTos { NombreServicio = "No debe crearse" });

        Assert.Null(resultado);
        Assert.Equal(1, await db.Servicios.CountAsync());
        Assert.Equal("Corte clasico", (await db.Servicios.SingleAsync()).NombreServicio);
    }

    [Fact]
    public async Task Eliminar_ExistenteYRepetir_ConfirmaAusencia()
    {
        Assert.True(await servicio.EliminarServicioAsync(1));
        db.ChangeTracker.Clear();

        Assert.Null(await db.Servicios.FindAsync(1));
        Assert.False(await servicio.EliminarServicioAsync(1));
        Assert.Empty(await db.Servicios.ToListAsync());
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
