using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-AUT-01: Login con credenciales válidas devuelve 200 y los datos del usuario")]
        public async Task Login_ValidCredentials_ReturnsOkWithUserData()
        {
            // Arrange
            var credenciales = new AuthDTos { Correo = "cliente@test.com", PasswordHash = "clave123" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", credenciales);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("cliente@test.com").And.Contain("Carlos").And.Contain("López");
        }

        [Fact(DisplayName = "IT-AUT-02: Login con contraseña incorrecta devuelve 401")]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var credenciales = new AuthDTos { Correo = "cliente@test.com", PasswordHash = "incorrecta" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", credenciales);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "IT-AUT-03: Login con correo inexistente devuelve 401")]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            // Arrange
            var credenciales = new AuthDTos { Correo = "nadie@test.com", PasswordHash = "clave123" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", credenciales);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
