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
    /// Pruebas unitarias del servicio de Usuarios.
    /// Verifica el mapeo con Roles (NombreRol) y las reglas de negocio
    /// (rol por defecto al crear y validación al cambiar de rol).
    /// </summary>
    public class UsuariosServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly UsuariosServices _service;

        public UsuariosServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new UsuariosServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact(DisplayName = "UT-USR-01: GetUsuariosAsync retorna la lista con el nombre del rol")]
        public async Task GetUsuariosAsync_ReturnsListWithRoleName()
        {
            // Arrange
            _context.Roles.Add(new Roles { RolId = 1, NombreRol = "Cliente" });
            _context.Usuarios.Add(new Usuarios { UsuariosId = 1, RolId = 1, Nombre = "Luis", Apellido = "Díaz", Correo = "luis@test.com", PasswordHash = "h" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUsuariosAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].NombreRol.Should().Be("Cliente");
        }

        [Fact(DisplayName = "UT-USR-02: GetUsuarioByIdAsync con Id existente retorna el usuario")]
        public async Task GetUsuarioByIdAsync_ExistingId_ReturnsUsuario()
        {
            // Arrange
            _context.Roles.Add(new Roles { RolId = 2, NombreRol = "Barbero" });
            _context.Usuarios.Add(new Usuarios { UsuariosId = 20, RolId = 2, Nombre = "Pedro", Apellido = "Ruiz", Correo = "pedro@test.com", PasswordHash = "h" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUsuarioByIdAsync(20);

            // Assert
            result.Should().NotBeNull();
            result!.UsuariosId.Should().Be(20);
            result.NombreRol.Should().Be("Barbero");
        }

        [Fact(DisplayName = "UT-USR-03: GetUsuarioByIdAsync con Id inexistente retorna null")]
        public async Task GetUsuarioByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetUsuarioByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-USR-04: CrearUsuarioAsync asigna el rol por defecto (1) al nuevo usuario")]
        public async Task CrearUsuarioAsync_AssignsDefaultRole()
        {
            // Arrange: se envía RolId = 3, pero el servicio debe forzar el rol 1 (cliente).
            var dto = new RegistroDTO
            {
                Nombre = "Nuevo",
                Apellido = "Cliente",
                Correo = "nuevo@test.com",
                PasswordHash = "hash",
                RolId = 3
            };

            // Act
            var result = await _service.CrearUsuarioAsync(dto);

            // Assert
            result.UsuariosId.Should().BeGreaterThan(0);
            var guardado = await _context.Usuarios.FindAsync(result.UsuariosId);
            guardado!.RolId.Should().Be(1);
        }

        [Fact(DisplayName = "UT-USR-05: CambiarRolAsync con usuario y rol válidos actualiza el rol")]
        public async Task CambiarRolAsync_ValidUserAndRole_UpdatesRole()
        {
            // Arrange
            _context.Roles.AddRange(
                new Roles { RolId = 1, NombreRol = "Cliente" },
                new Roles { RolId = 3, NombreRol = "Administrador" });
            _context.Usuarios.Add(new Usuarios { UsuariosId = 30, RolId = 1, Nombre = "Mario", Apellido = "Soto", Correo = "mario@test.com", PasswordHash = "h" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CambiarRolAsync(30, 3);

            // Assert
            result.Should().NotBeNull();
            result!.NombreRol.Should().Be("Administrador");
            (await _context.Usuarios.FindAsync(30))!.RolId.Should().Be(3);
        }

        [Fact(DisplayName = "UT-USR-06: CambiarRolAsync con usuario inexistente retorna null")]
        public async Task CambiarRolAsync_NonExistentUser_ReturnsNull()
        {
            // Arrange
            _context.Roles.Add(new Roles { RolId = 3, NombreRol = "Administrador" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CambiarRolAsync(8888, 3);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-USR-07: CambiarRolAsync con rol inexistente retorna null")]
        public async Task CambiarRolAsync_NonExistentRole_ReturnsNull()
        {
            // Arrange
            _context.Usuarios.Add(new Usuarios { UsuariosId = 31, RolId = 1, Nombre = "R", Correo = "r@test.com", PasswordHash = "h" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CambiarRolAsync(31, 99);

            // Assert
            result.Should().BeNull();
        }
    }
}
