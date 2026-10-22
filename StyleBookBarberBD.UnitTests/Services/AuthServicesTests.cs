using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias del servicio de autenticación.
    /// Valida la búsqueda de usuario por correo y contraseña.
    /// </summary>
    public class AuthServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly AuthServices _service;

        public AuthServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);
            _service = new AuthServices(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact(DisplayName = "UT-AUT-01: LoginAsync con credenciales válidas retorna el usuario")]
        public async Task LoginAsync_ValidCredentials_ReturnsUsuario()
        {
            // Arrange
            _context.Usuarios.Add(new Usuarios
            {
                UsuariosId = 1,
                RolId = 3,
                Nombre = "Admin",
                Apellido = "General",
                Correo = "admin@stylebook.com",
                PasswordHash = "ClaveSegura2026*"
            });
            await _context.SaveChangesAsync();

            var dto = new AuthDTos { Correo = "admin@stylebook.com", PasswordHash = "ClaveSegura2026*" };

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.UsuariosId.Should().Be(1);
            result.Correo.Should().Be("admin@stylebook.com");
        }

        [Fact(DisplayName = "UT-AUT-02: LoginAsync con contraseña incorrecta retorna null")]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            // Arrange
            _context.Usuarios.Add(new Usuarios { UsuariosId = 2, RolId = 1, Nombre = "Cli", Apellido = "Uno", Correo = "cli@test.com", PasswordHash = "Real123" });
            await _context.SaveChangesAsync();

            var dto = new AuthDTos { Correo = "cli@test.com", PasswordHash = "Incorrecta" };

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-AUT-03: LoginAsync con correo inexistente retorna null")]
        public async Task LoginAsync_UnknownEmail_ReturnsNull()
        {
            // Arrange
            var dto = new AuthDTos { Correo = "noexiste@test.com", PasswordHash = "cualquiera" };

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            result.Should().BeNull();
        }
    }
}
