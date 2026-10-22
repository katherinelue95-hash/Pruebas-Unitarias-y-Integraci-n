using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias del servicio de Categorias (CRUD completo).
    /// </summary>
    public class CategoriasServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly CategoriasServices _service;

        public CategoriasServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new CategoriasServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact(DisplayName = "UT-CAT-01: GetCategoriasAsync retorna todas las categorías")]
        public async Task GetCategoriasAsync_ReturnsAll()
        {
            // Arrange
            _context.Categorias.AddRange(
                new Categorias { CategoriasId = 1, NombreCategoria = "Cortes", Descripcion = "Cortes" },
                new Categorias { CategoriasId = 2, NombreCategoria = "Barba", Descripcion = "Barba" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCategoriasAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact(DisplayName = "UT-CAT-02: GetCategoriaByIdAsync con Id existente retorna la categoría")]
        public async Task GetCategoriaByIdAsync_ExistingId_ReturnsCategoria()
        {
            // Arrange
            _context.Categorias.Add(new Categorias { CategoriasId = 7, NombreCategoria = "Color", Descripcion = "Tintes" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCategoriaByIdAsync(7);

            // Assert
            result.Should().NotBeNull();
            result!.CategoriasId.Should().Be(7);
            result.NombreCategoria.Should().Be("Color");
        }

        [Fact(DisplayName = "UT-CAT-03: GetCategoriaByIdAsync con Id inexistente retorna null")]
        public async Task GetCategoriaByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetCategoriaByIdAsync(4040);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CAT-04: CrearCategoriaAsync inserta la categoría y retorna el DTO")]
        public async Task CrearCategoriaAsync_ValidDto_InsertsCategoria()
        {
            // Arrange
            var dto = new CategoriasDTos { NombreCategoria = "Uñas", Descripcion = "Manicure" };

            // Act
            var result = await _service.CrearCategoriaAsync(dto);

            // Assert
            result.CategoriasId.Should().BeGreaterThan(0);
            result.NombreCategoria.Should().Be("Uñas");
            (await _context.Categorias.FindAsync(result.CategoriasId)).Should().NotBeNull();
        }

        [Fact(DisplayName = "UT-CAT-05: ActualizarCategoriaAsync con Id existente actualiza los datos")]
        public async Task ActualizarCategoriaAsync_ExistingId_UpdatesCategoria()
        {
            // Arrange
            _context.Categorias.Add(new Categorias { CategoriasId = 9, NombreCategoria = "Viejo", Descripcion = "Viejo" });
            await _context.SaveChangesAsync();
            var dto = new CategoriasDTos { NombreCategoria = "Actualizado", Descripcion = "Nueva descripción" };

            // Act
            var result = await _service.ActualizarCategoriaAsync(9, dto);

            // Assert
            result.Should().NotBeNull();
            result!.NombreCategoria.Should().Be("Actualizado");
            (await _context.Categorias.FindAsync(9))!.Descripcion.Should().Be("Nueva descripción");
        }

        [Fact(DisplayName = "UT-CAT-06: ActualizarCategoriaAsync con Id inexistente retorna null")]
        public async Task ActualizarCategoriaAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var dto = new CategoriasDTos { NombreCategoria = "X" };

            // Act
            var result = await _service.ActualizarCategoriaAsync(5000, dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CAT-07: EliminarCategoriaAsync con Id existente elimina y retorna true")]
        public async Task EliminarCategoriaAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _context.Categorias.Add(new Categorias { CategoriasId = 3, NombreCategoria = "Borrar" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarCategoriaAsync(3);

            // Assert
            result.Should().BeTrue();
            (await _context.Categorias.FindAsync(3)).Should().BeNull();
        }

        [Fact(DisplayName = "UT-CAT-08: EliminarCategoriaAsync con Id inexistente retorna false")]
        public async Task EliminarCategoriaAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarCategoriaAsync(6060);

            // Assert
            result.Should().BeFalse();
        }
    }
}
