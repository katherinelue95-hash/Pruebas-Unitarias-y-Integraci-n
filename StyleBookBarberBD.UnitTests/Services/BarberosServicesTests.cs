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
    /// Pruebas unitarias del servicio de Barberos (CRUD).
    /// Verifica el mapeo NombreCompleto/Correo desde la entidad Usuario relacionada.
    /// </summary>
    public class BarberosServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly BarberosServices _service;

        public BarberosServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new BarberosServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedUsuarioAsync(int id, string nombre, string apellido, string correo)
        {
            _context.Usuarios.Add(new Usuarios
            {
                UsuariosId = id,
                RolId = 2,
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                PasswordHash = "hash"
            });
            await _context.SaveChangesAsync();
        }

        [Fact(DisplayName = "UT-BAR-01: GetBarberosAsync retorna la lista con el nombre completo del usuario")]
        public async Task GetBarberosAsync_ReturnsListWithUserName()
        {
            // Arrange
            await SeedUsuarioAsync(1, "Juan", "Pérez", "juan@test.com");
            _context.Barberos.Add(new Barberos { BarberosId = 1, UsuariosId = 1, Especialidad = "Fade", Calificacion = 4.5m });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBarberosAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].NombreCompleto.Should().Be("Juan Pérez");
            result[0].Correo.Should().Be("juan@test.com");
        }

        [Fact(DisplayName = "UT-BAR-02: GetBarberoByIdAsync con Id existente retorna el barbero")]
        public async Task GetBarberoByIdAsync_ExistingId_ReturnsBarbero()
        {
            // Arrange
            await SeedUsuarioAsync(2, "Ana", "Gómez", "ana@test.com");
            _context.Barberos.Add(new Barberos { BarberosId = 4, UsuariosId = 2, Especialidad = "Color", Calificacion = 5m });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBarberoByIdAsync(4);

            // Assert
            result.Should().NotBeNull();
            result!.BarberosId.Should().Be(4);
            result.UsuarioId.Should().Be(2);
            result.NombreCompleto.Should().Be("Ana Gómez");
        }

        [Fact(DisplayName = "UT-BAR-03: GetBarberoByIdAsync con Id inexistente retorna null")]
        public async Task GetBarberoByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetBarberoByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-BAR-04: CrearBarberoAsync mapea UsuarioId a UsuariosId e inserta")]
        public async Task CrearBarberoAsync_ValidDto_MapsUsuarioIdAndInserts()
        {
            // Arrange
            var dto = new BarberosDTos { UsuarioId = 7, Especialidad = "Barba", Calificacion = 4m, EstadoDisp = "Disponible", FotoUrl = "foto.jpg" };

            // Act
            var result = await _service.CrearBarberoAsync(dto);

            // Assert
            result.BarberosId.Should().BeGreaterThan(0);
            var guardado = await _context.Barberos.FindAsync(result.BarberosId);
            guardado.Should().NotBeNull();
            guardado!.UsuariosId.Should().Be(7);
            guardado.Especialidad.Should().Be("Barba");
        }

        [Fact(DisplayName = "UT-BAR-05: ActualizarBarberoAsync con Id existente actualiza los datos")]
        public async Task ActualizarBarberoAsync_ExistingId_UpdatesBarbero()
        {
            // Arrange
            _context.Barberos.Add(new Barberos { BarberosId = 6, UsuariosId = 1, Especialidad = "Viejo", Calificacion = 3m });
            await _context.SaveChangesAsync();
            var dto = new BarberosDTos { UsuarioId = 1, Especialidad = "Nuevo", Calificacion = 4.8m, EstadoDisp = "Ocupado", FotoUrl = "f.jpg" };

            // Act
            var result = await _service.ActualizarBarberoAsync(6, dto);

            // Assert
            result.Should().NotBeNull();
            result!.Especialidad.Should().Be("Nuevo");
            var actualizado = await _context.Barberos.FindAsync(6);
            actualizado!.EstadoDisp.Should().Be("Ocupado");
            actualizado.Calificacion.Should().Be(4.8m);
        }

        [Fact(DisplayName = "UT-BAR-06: ActualizarBarberoAsync con Id inexistente retorna null")]
        public async Task ActualizarBarberoAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var dto = new BarberosDTos { UsuarioId = 1, Especialidad = "X" };

            // Act
            var result = await _service.ActualizarBarberoAsync(5000, dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-BAR-07: EliminarBarberoAsync con Id existente elimina y retorna true")]
        public async Task EliminarBarberoAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _context.Barberos.Add(new Barberos { BarberosId = 9, UsuariosId = 1, Especialidad = "Borrar" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarBarberoAsync(9);

            // Assert
            result.Should().BeTrue();
            (await _context.Barberos.FindAsync(9)).Should().BeNull();
        }

        [Fact(DisplayName = "UT-BAR-08: EliminarBarberoAsync con Id inexistente retorna false")]
        public async Task EliminarBarberoAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarBarberoAsync(7070);

            // Assert
            result.Should().BeFalse();
        }
    }
}
