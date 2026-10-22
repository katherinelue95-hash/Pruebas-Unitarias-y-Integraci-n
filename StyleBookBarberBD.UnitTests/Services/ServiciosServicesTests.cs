using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias del servicio de Servicios.
    /// Se usa una base de datos en memoria (EF Core InMemory) igual que en el ejemplo,
    /// para aislar cada prueba sin depender de SQL Server.
    /// </summary>
    public class ServiciosServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly ServiciosServices _service;

        public ServiciosServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new ServiciosServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact(DisplayName = "UT-SRV-01: GetServiciosAsync retorna la lista con el nombre de la categoría")]
        // Propósito: Verificar que el listado mapee la relación con Categorias (NombreCategoria).
        public async Task GetServiciosAsync_ReturnsListWithCategoryName()
        {
            // Arrange
            _context.Categorias.Add(new Categorias { CategoriasId = 1, NombreCategoria = "Cortes", Descripcion = "Cortes de cabello" });
            _context.Servicios.AddRange(
                new Servicios { ServiciosId = 1, CategoriaId = 1, NombreServicio = "Corte clásico", Precio = 8000m, DuracionMin = 30 },
                new Servicios { ServiciosId = 2, CategoriaId = 1, NombreServicio = "Corte + barba", Precio = 12000m, DuracionMin = 45 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetServiciosAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.NombreCategoria == "Cortes");
        }

        [Fact(DisplayName = "UT-SRV-02: GetServicioByIdAsync con Id existente retorna el servicio")]
        public async Task GetServicioByIdAsync_ExistingId_ReturnsServicio()
        {
            // Arrange
            _context.Categorias.Add(new Categorias { CategoriasId = 1, NombreCategoria = "Barba" });
            _context.Servicios.Add(new Servicios { ServiciosId = 5, CategoriaId = 1, NombreServicio = "Afeitado", Precio = 5000m, DuracionMin = 20 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetServicioByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.ServiciosId.Should().Be(5);
            result.NombreServicio.Should().Be("Afeitado");
            result.NombreCategoria.Should().Be("Barba");
        }

        [Fact(DisplayName = "UT-SRV-03: GetServicioByIdAsync con Id inexistente retorna null")]
        public async Task GetServicioByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetServicioByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-SRV-04: CrearServicioAsync inserta el servicio y retorna el DTO con Id")]
        public async Task CrearServicioAsync_ValidDto_InsertsServicio()
        {
            // Arrange
            var dto = new ServiciosDTos { CategoriaId = 1, NombreServicio = "Tinte", Precio = 15000m, DuracionMin = 60 };

            // Act
            var result = await _service.CrearServicioAsync(dto);

            // Assert
            result.ServiciosId.Should().BeGreaterThan(0);
            result.NombreServicio.Should().Be("Tinte");
            var guardado = await _context.Servicios.FindAsync(result.ServiciosId);
            guardado.Should().NotBeNull();
            guardado!.Precio.Should().Be(15000m);
        }

        [Fact(DisplayName = "UT-SRV-05: ActualizarServicioAsync con Id existente actualiza los datos")]
        public async Task ActualizarServicioAsync_ExistingId_UpdatesServicio()
        {
            // Arrange
            _context.Servicios.Add(new Servicios { ServiciosId = 8, CategoriaId = 1, NombreServicio = "Antiguo", Precio = 1000m, DuracionMin = 10 });
            await _context.SaveChangesAsync();

            var dto = new ServiciosDTos { CategoriaId = 2, NombreServicio = "Nuevo", Precio = 2000m, DuracionMin = 25, FotoUrl = "foto.jpg" };

            // Act
            var result = await _service.ActualizarServicioAsync(8, dto);

            // Assert
            result.Should().NotBeNull();
            result!.NombreServicio.Should().Be("Nuevo");
            var actualizado = await _context.Servicios.FindAsync(8);
            actualizado!.Precio.Should().Be(2000m);
            actualizado.CategoriaId.Should().Be(2);
        }

        [Fact(DisplayName = "UT-SRV-06: ActualizarServicioAsync con Id inexistente retorna null")]
        public async Task ActualizarServicioAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var dto = new ServiciosDTos { CategoriaId = 1, NombreServicio = "X", Precio = 1m, DuracionMin = 1 };

            // Act
            var result = await _service.ActualizarServicioAsync(4040, dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-SRV-07: EliminarServicioAsync con Id existente elimina y retorna true")]
        public async Task EliminarServicioAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _context.Servicios.Add(new Servicios { ServiciosId = 3, CategoriaId = 1, NombreServicio = "Borrar", Precio = 1m, DuracionMin = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarServicioAsync(3);

            // Assert
            result.Should().BeTrue();
            (await _context.Servicios.FindAsync(3)).Should().BeNull();
        }

        [Fact(DisplayName = "UT-SRV-08: EliminarServicioAsync con Id inexistente retorna false")]
        public async Task EliminarServicioAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarServicioAsync(7777);

            // Assert
            result.Should().BeFalse();
        }
    }
}
