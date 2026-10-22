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
    /// Pruebas unitarias del servicio de Resenas.
    /// Cubre el mapeo del autor, las validaciones (estrellas 1-5, usuario y barbero reales)
    /// y el recálculo del promedio del barbero al crear o eliminar una reseña.
    /// </summary>
    public class ResenasServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly ResenasServices _service;

        public ResenasServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new ResenasServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedUsuarioYBarberoAsync(int barberoId = 1, decimal calificacion = 0m)
        {
            _context.Usuarios.Add(new Usuarios { UsuariosId = 1, RolId = 1, Nombre = "Lucía", Apellido = "Fernández", Correo = "l@test.com", PasswordHash = "h" });
            _context.Usuarios.Add(new Usuarios { UsuariosId = 2, RolId = 2, Nombre = "Marco", Apellido = "Rojas", Correo = "m@test.com", PasswordHash = "h" });
            _context.Barberos.Add(new Barberos { BarberosId = barberoId, UsuariosId = 2, Especialidad = "Fade", Calificacion = calificacion });
            await _context.SaveChangesAsync();
        }

        [Fact(DisplayName = "UT-RES-01: GetResenasAsync retorna las reseñas con autor y promedio")]
        public async Task GetResenasAsync_ReturnsListWithAutorAndPromedio()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync(calificacion: 4.5m);
            _context.Resenas.Add(new Resenas { ResenaId = 1, BarberosId = 1, UsuariosId = 1, Estrellas = 4, Comentario = "Buen corte", Fecha = DateTime.Now });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetResenasAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Autor.Should().Be("Lucía Fernández");
            result[0].CalificacionPromedio.Should().Be(4.5m);
        }

        [Fact(DisplayName = "UT-RES-02: GetResenasByBarberoAsync filtra por barbero")]
        public async Task GetResenasByBarberoAsync_FiltersByBarbero()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync(barberoId: 1);
            _context.Barberos.Add(new Barberos { BarberosId = 2, UsuariosId = 2, Especialidad = "Color", Calificacion = 3m });
            _context.Resenas.AddRange(
                new Resenas { ResenaId = 1, BarberosId = 1, UsuariosId = 1, Estrellas = 5, Fecha = DateTime.Now },
                new Resenas { ResenaId = 2, BarberosId = 2, UsuariosId = 1, Estrellas = 2, Fecha = DateTime.Now });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetResenasByBarberoAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result[0].BarberosId.Should().Be(1);
        }

        [Fact(DisplayName = "UT-RES-03: CrearResenaAsync válida crea la reseña y actualiza el promedio del barbero")]
        public async Task CrearResenaAsync_Valid_CreatesAndUpdatesAverage()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync(calificacion: 3m);
            _context.Resenas.Add(new Resenas { ResenaId = 1, BarberosId = 1, UsuariosId = 1, Estrellas = 3, Fecha = DateTime.Now });
            await _context.SaveChangesAsync();

            var dto = new ResenasDTos { BarberosId = 1, UsuariosId = 1, Estrellas = 5, Comentario = "Excelente" };

            // Act
            var result = await _service.CrearResenaAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.ResenaId.Should().BeGreaterThan(0);
            result.Autor.Should().Be("Lucía Fernández");
            result.CalificacionPromedio.Should().Be(4m); // (3 + 5) / 2
            (await _context.Barberos.FindAsync(1))!.Calificacion.Should().Be(4m);
        }

        [Fact(DisplayName = "UT-RES-04: CrearResenaAsync con estrellas fuera de rango (0) retorna null")]
        public async Task CrearResenaAsync_EstrellasCero_ReturnsNull()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync();
            var dto = new ResenasDTos { BarberosId = 1, UsuariosId = 1, Estrellas = 0 };

            // Act
            var result = await _service.CrearResenaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-RES-05: CrearResenaAsync con estrellas fuera de rango (6) retorna null")]
        public async Task CrearResenaAsync_EstrellasSeis_ReturnsNull()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync();
            var dto = new ResenasDTos { BarberosId = 1, UsuariosId = 1, Estrellas = 6 };

            // Act
            var result = await _service.CrearResenaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-RES-06: CrearResenaAsync con usuario inexistente retorna null")]
        public async Task CrearResenaAsync_NonExistentUsuario_ReturnsNull()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync();
            var dto = new ResenasDTos { BarberosId = 1, UsuariosId = 999, Estrellas = 4 };

            // Act
            var result = await _service.CrearResenaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-RES-07: CrearResenaAsync con barbero inexistente retorna null")]
        public async Task CrearResenaAsync_NonExistentBarbero_ReturnsNull()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync();
            var dto = new ResenasDTos { BarberosId = 999, UsuariosId = 1, Estrellas = 4 };

            // Act
            var result = await _service.CrearResenaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-RES-08: EliminarResenaAsync existente elimina y recalcula el promedio")]
        public async Task EliminarResenaAsync_Existing_RecalculatesAverage()
        {
            // Arrange
            await SeedUsuarioYBarberoAsync(calificacion: 4m);
            _context.Resenas.AddRange(
                new Resenas { ResenaId = 1, BarberosId = 1, UsuariosId = 1, Estrellas = 5, Fecha = DateTime.Now },
                new Resenas { ResenaId = 2, BarberosId = 1, UsuariosId = 1, Estrellas = 3, Fecha = DateTime.Now });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarResenaAsync(2);

            // Assert
            result.Should().BeTrue();
            (await _context.Resenas.FindAsync(2)).Should().BeNull();
            (await _context.Barberos.FindAsync(1))!.Calificacion.Should().Be(5m); // solo queda la de 5
        }

        [Fact(DisplayName = "UT-RES-09: EliminarResenaAsync con Id inexistente retorna false")]
        public async Task EliminarResenaAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarResenaAsync(4040);

            // Assert
            result.Should().BeFalse();
        }
    }
}
