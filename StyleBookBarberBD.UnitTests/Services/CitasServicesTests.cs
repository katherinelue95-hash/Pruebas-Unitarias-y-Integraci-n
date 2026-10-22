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
    /// Pruebas unitarias del servicio de Citas.
    /// Cubre el mapeo de nombres, las validaciones de existencia de cliente/barbero/servicio,
    /// el control de horario ocupado, el cambio de estado y la eliminación.
    /// </summary>
    public class CitasServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly CitasServices _service;

        public CitasServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new CitasServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedBaseAsync()
        {
            _context.Usuarios.Add(new Usuarios { UsuariosId = 1, RolId = 1, Nombre = "Carlos", Apellido = "López", Correo = "c@test.com", PasswordHash = "h" });
            _context.Usuarios.Add(new Usuarios { UsuariosId = 2, RolId = 2, Nombre = "Andrés", Apellido = "Mora", Correo = "a@test.com", PasswordHash = "h" });
            _context.Barberos.Add(new Barberos { BarberosId = 1, UsuariosId = 2, Especialidad = "Fade", Calificacion = 4m });
            _context.Servicios.Add(new Servicios { ServiciosId = 1, CategoriaId = 1, NombreServicio = "Corte", Precio = 8000m, DuracionMin = 30 });
            await _context.SaveChangesAsync();
        }

        [Fact(DisplayName = "UT-CIT-01: GetCitasAsync retorna las citas con los nombres relacionados")]
        public async Task GetCitasAsync_ReturnsListWithNames()
        {
            // Arrange
            await SeedBaseAsync();
            _context.Citas.Add(new Citas { CitasId = 1, ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 5, 1, 10, 0, 0), MontoTotal = 8000m, Estado = "Pendiente" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCitasAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].ClienteNombre.Should().Be("Carlos López");
            result[0].BarberoNombre.Should().Be("Andrés Mora");
            result[0].ServicioNombre.Should().Be("Corte");
        }

        [Fact(DisplayName = "UT-CIT-02: GetCitaByIdAsync con Id existente retorna la cita")]
        public async Task GetCitaByIdAsync_ExistingId_ReturnsCita()
        {
            // Arrange
            await SeedBaseAsync();
            _context.Citas.Add(new Citas { CitasId = 10, ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 5, 2, 11, 0, 0), MontoTotal = 8000m });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCitaByIdAsync(10);

            // Assert
            result.Should().NotBeNull();
            result!.CitasId.Should().Be(10);
            result.BarberoNombre.Should().Be("Andrés Mora");
        }

        [Fact(DisplayName = "UT-CIT-03: GetCitaByIdAsync con Id inexistente retorna null")]
        public async Task GetCitaByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetCitaByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-04: CrearCitaAsync con datos válidos crea la cita en estado Pendiente")]
        public async Task CrearCitaAsync_ValidData_CreatesPendiente()
        {
            // Arrange
            await SeedBaseAsync();
            var dto = new CitasDTos { ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 6, 1, 9, 0, 0), MontoTotal = 8000m };

            // Act
            var result = await _service.CrearCitaAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.CitasId.Should().BeGreaterThan(0);
            result.Estado.Should().Be("Pendiente");
            result.ClienteNombre.Should().Be("Carlos López");
        }

        [Fact(DisplayName = "UT-CIT-05: CrearCitaAsync con cliente inexistente retorna null")]
        public async Task CrearCitaAsync_NonExistentCliente_ReturnsNull()
        {
            // Arrange
            await SeedBaseAsync();
            var dto = new CitasDTos { ClienteId = 999, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 6, 2, 9, 0, 0) };

            // Act
            var result = await _service.CrearCitaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-06: CrearCitaAsync con barbero inexistente retorna null")]
        public async Task CrearCitaAsync_NonExistentBarbero_ReturnsNull()
        {
            // Arrange
            await SeedBaseAsync();
            var dto = new CitasDTos { ClienteId = 1, BarberosId = 999, ServiciosId = 1, FechaHora = new DateTime(2026, 6, 3, 9, 0, 0) };

            // Act
            var result = await _service.CrearCitaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-07: CrearCitaAsync con servicio inexistente retorna null")]
        public async Task CrearCitaAsync_NonExistentServicio_ReturnsNull()
        {
            // Arrange
            await SeedBaseAsync();
            var dto = new CitasDTos { ClienteId = 1, BarberosId = 1, ServiciosId = 999, FechaHora = new DateTime(2026, 6, 4, 9, 0, 0) };

            // Act
            var result = await _service.CrearCitaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-08: CrearCitaAsync con horario ocupado retorna null")]
        public async Task CrearCitaAsync_OccupiedSlot_ReturnsNull()
        {
            // Arrange
            await SeedBaseAsync();
            var fecha = new DateTime(2026, 7, 1, 10, 0, 0);
            _context.Citas.Add(new Citas { CitasId = 1, ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = fecha });
            await _context.SaveChangesAsync();

            var dto = new CitasDTos { ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = fecha };

            // Act
            var result = await _service.CrearCitaAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-09: CambiarEstadoAsync con estado válido actualiza la cita")]
        public async Task CambiarEstadoAsync_ValidState_UpdatesCita()
        {
            // Arrange
            await SeedBaseAsync();
            _context.Citas.Add(new Citas { CitasId = 20, ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 8, 1, 10, 0, 0) });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CambiarEstadoAsync(20, "Confirmada");

            // Assert
            result.Should().NotBeNull();
            result!.Estado.Should().Be("Confirmada");
            (await _context.Citas.FindAsync(20))!.Estado.Should().Be("Confirmada");
        }

        [Fact(DisplayName = "UT-CIT-10: CambiarEstadoAsync con estado inválido retorna null")]
        public async Task CambiarEstadoAsync_InvalidState_ReturnsNull()
        {
            // Arrange
            await SeedBaseAsync();
            _context.Citas.Add(new Citas { CitasId = 21, ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 8, 2, 10, 0, 0) });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CambiarEstadoAsync(21, "EstadoInventado");

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-11: CambiarEstadoAsync con cita inexistente retorna null")]
        public async Task CambiarEstadoAsync_NonExistentCita_ReturnsNull()
        {
            // Act
            var result = await _service.CambiarEstadoAsync(9999, "Confirmada");

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-12: EliminarCitaAsync con Id existente elimina y retorna true")]
        public async Task EliminarCitaAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            await SeedBaseAsync();
            _context.Citas.Add(new Citas { CitasId = 30, ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 9, 1, 10, 0, 0) });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarCitaAsync(30);

            // Assert
            result.Should().BeTrue();
            (await _context.Citas.FindAsync(30)).Should().BeNull();
        }

        [Fact(DisplayName = "UT-CIT-13: EliminarCitaAsync con Id inexistente retorna false")]
        public async Task EliminarCitaAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarCitaAsync(4040);

            // Assert
            result.Should().BeFalse();
        }
    }
}
