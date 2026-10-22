using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests;

public class CategoriasServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly CategoriasServices servicio;

    public CategoriasServicesTests()
    {
        var opciones =
            new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        db = new StyleBookBarberBDContext(opciones);

        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);

        servicio = new CategoriasServices(
            db,
            configuracion.CreateMapper());
    }

    private Categorias CrearRegistro(
        int id,
        string nombre,
        string descripcion)
    {
        var categoria = new Categorias
        {
            CategoriasId = id,
            NombreCategoria = nombre,
            Descripcion = descripcion
        };

        db.Categorias.Add(categoria);
        db.SaveChanges();

        return categoria;
    }

    private CategoriasDTos NuevaCategoria() => new()
    {
        NombreCategoria = "Cortes de cabello",
        Descripcion = "Servicios de corte"
    };

    [Fact]
    public async Task ConsultarCategorias_SinRegistros_RetornaListaVacia()
    {
        var resultado = await servicio.GetCategoriasAsync();

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ConsultarCategorias_ConRegistros_RetornaTodas()
    {
        CrearRegistro(
            10,
            "Cortes",
            "Cortes de cabello");

        CrearRegistro(
            20,
            "Barba",
            "Servicios para barba");

        var resultado = await servicio.GetCategoriasAsync();

        Assert.Equal(2, resultado.Count);

        Assert.Contains(
            resultado,
            c => c.CategoriasId == 10);

        Assert.Contains(
            resultado,
            c => c.CategoriasId == 20);
    }

    [Fact]
    public async Task ConsultarPorId_Existente_RetornaCategoria()
    {
        CrearRegistro(
            10,
            "Cortes",
            "Servicios de cortes");

        var resultado =
            await servicio.GetCategoriaByIdAsync(10);

        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.CategoriasId);
        Assert.Equal("Cortes", resultado.NombreCategoria);
        Assert.Equal(
            "Servicios de cortes",
            resultado.Descripcion);
    }

    [Fact]
    public async Task ConsultarPorId_Inexistente_RetornaNull()
    {
        var resultado =
            await servicio.GetCategoriaByIdAsync(999);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task CrearCategoria_Valida_GuardaRegistro()
    {
        var dto = NuevaCategoria();

        var resultado =
            await servicio.CrearCategoriaAsync(dto);

        Assert.NotNull(resultado);
        Assert.True(resultado.CategoriasId > 0);

        var guardada =
            await db.Categorias.FindAsync(resultado.CategoriasId);

        Assert.NotNull(guardada);

        Assert.Equal(
            "Cortes de cabello",
            guardada.NombreCategoria);

        Assert.Equal(
            "Servicios de corte",
            guardada.Descripcion);

        Assert.Equal(1, await db.Categorias.CountAsync());
    }

    [Fact]
    public async Task CrearDosCategorias_GuardaAmbosRegistros()
    {
        var primera = NuevaCategoria();

        var segunda = new CategoriasDTos
        {
            NombreCategoria = "Cuidado de barba",
            Descripcion = "Afeitado y perfilado"
        };

        var resultado1 =
            await servicio.CrearCategoriaAsync(primera);

        var resultado2 =
            await servicio.CrearCategoriaAsync(segunda);

        Assert.NotEqual(
            resultado1.CategoriasId,
            resultado2.CategoriasId);

        Assert.Equal(2, await db.Categorias.CountAsync());
    }

    [Fact]
    public async Task ActualizarCategoria_Existente_GuardaCambios()
    {
        CrearRegistro(
            10,
            "Cortes",
            "Descripcion anterior");

        var dto = new CategoriasDTos
        {
            CategoriasId = 999,
            NombreCategoria = "Cortes modernos",
            Descripcion = "Degradados y estilos"
        };

        var resultado =
            await servicio.ActualizarCategoriaAsync(10, dto);

        Assert.NotNull(resultado);

        var guardada = await db.Categorias.FindAsync(10);

        Assert.NotNull(guardada);
        Assert.Equal(10, guardada.CategoriasId);

        Assert.Equal(
            "Cortes modernos",
            guardada.NombreCategoria);

        Assert.Equal(
            "Degradados y estilos",
            guardada.Descripcion);

        Assert.Equal(1, await db.Categorias.CountAsync());
    }

    [Fact]
    public async Task ActualizarCategoria_Inexistente_RetornaNull()
    {
        var dto = NuevaCategoria();

        var resultado =
            await servicio.ActualizarCategoriaAsync(999, dto);

        Assert.Null(resultado);
        Assert.Empty(await db.Categorias.ToListAsync());
    }

    [Fact]
    public async Task EliminarCategoria_Existente_EliminaRegistro()
    {
        CrearRegistro(
            10,
            "Cortes",
            "Servicios de corte");

        var resultado =
            await servicio.EliminarCategoriaAsync(10);

        Assert.True(resultado);

        Assert.Null(
            await db.Categorias.FindAsync(10));

        Assert.Empty(
            await db.Categorias.ToListAsync());
    }

    [Fact]
    public async Task EliminarCategoria_Inexistente_RetornaFalse()
    {
        var resultado =
            await servicio.EliminarCategoriaAsync(999);

        Assert.False(resultado);
        Assert.Empty(await db.Categorias.ToListAsync());
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
