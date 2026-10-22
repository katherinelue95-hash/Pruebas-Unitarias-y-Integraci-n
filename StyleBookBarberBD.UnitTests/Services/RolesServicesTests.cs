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
    /// Pruebas unitarias del servicio de Roles (CRUD completo).
    /// Incluye la verificación del mapeo especial RolId &lt;-&gt; RolesId.
    /// </summary>
    public class RolesServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly RolesServices _service;

        public RolesServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new RolesServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact(DisplayName = "UT-ROL-01: GetRolesAsync retorna todos los roles mapeando RolesId")]
        public async Task GetRolesAsync_ReturnsAll()
        {
            // Arrange
            _context.Roles.AddRange(
                new Roles { RolId = 1, NombreRol = "Cliente" },
                new Roles { RolId = 2, NombreRol = "Barbero" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRolesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.RolesId == 1 && r.NombreRol == "Cliente");
        }

        [Fact(DisplayName = "UT-ROL-02: GetRolByIdAsync con Id existente retorna el rol")]
        public async Task GetRolByIdAsync_ExistingId_ReturnsRol()
        {
            // Arrange
            _context.Roles.Add(new Roles { RolId = 3, NombreRol = "Administrador" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRolByIdAsync(3);

            // Assert
            result.Should().NotBeNull();
            result!.RolesId.Should().Be(3);
            result.NombreRol.Should().Be("Administrador");
        }

        [Fact(DisplayName = "UT-ROL-03: GetRolByIdAsync con Id inexistente retorna null")]
        public async Task GetRolByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetRolByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-ROL-04: CrearRolAsync inserta el rol mapeando RolesId a RolId")]
        public async Task CrearRolAsync_ValidDto_InsertsRol()
        {
            // Arrange
            var dto = new RolesDTos { NombreRol = "Supervisor" };

            // Act
            var result = await _service.CrearRolAsync(dto);

            // Assert
            result.RolesId.Should().BeGreaterThan(0);
            result.NombreRol.Should().Be("Supervisor");
            (await _context.Roles.FindAsync(result.RolesId)).Should().NotBeNull();
        }

        [Fact(DisplayName = "UT-ROL-05: ActualizarRolAsync con Id existente actualiza el nombre")]
        public async Task ActualizarRolAsync_ExistingId_UpdatesRol()
        {
            // Arrange
            _context.Roles.Add(new Roles { RolId = 4, NombreRol = "Viejo" });
            await _context.SaveChangesAsync();
            var dto = new RolesDTos { NombreRol = "Nuevo" };

            // Act
            var result = await _service.ActualizarRolAsync(4, dto);

            // Assert
            result.Should().NotBeNull();
            result!.NombreRol.Should().Be("Nuevo");
            (await _context.Roles.FindAsync(4))!.NombreRol.Should().Be("Nuevo");
        }

        [Fact(DisplayName = "UT-ROL-06: ActualizarRolAsync con Id inexistente retorna null")]
        public async Task ActualizarRolAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.ActualizarRolAsync(8888, new RolesDTos { NombreRol = "X" });

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-ROL-07: EliminarRolAsync con Id existente elimina y retorna true")]
        public async Task EliminarRolAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _context.Roles.Add(new Roles { RolId = 6, NombreRol = "Borrar" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarRolAsync(6);

            // Assert
            result.Should().BeTrue();
            (await _context.Roles.FindAsync(6)).Should().BeNull();
        }

        [Fact(DisplayName = "UT-ROL-08: EliminarRolAsync con Id inexistente retorna false")]
        public async Task EliminarRolAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarRolAsync(7070);

            // Assert
            result.Should().BeFalse();
        }
    }
}
