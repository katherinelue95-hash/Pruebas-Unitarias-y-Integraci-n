using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class UsuariosEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsuariosEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-USR-01: GET /api/usuarios devuelve 200 con la lista de usuarios")]
        public async Task GetUsuarios_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/usuarios");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var usuarios = await response.Content.ReadFromJsonAsync<List<RegistroDTO>>();
            usuarios.Should().NotBeNull();
            usuarios!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-USR-02: GET /api/usuarios/{id} existente devuelve 200")]
        public async Task GetUsuarioById_Existing_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/usuarios/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var usuario = await response.Content.ReadFromJsonAsync<RegistroDTO>();
            usuario!.UsuariosId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-USR-03: GET /api/usuarios/{id} inexistente devuelve 404")]
        public async Task GetUsuarioById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/usuarios/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-USR-04: POST /api/usuarios devuelve 201 y asigna el rol Cliente (1)")]
        public async Task CreateUsuario_Valid_ReturnsCreatedWithClienteRole()
        {
            // Arrange
            var dto = new RegistroDTO
            {
                Nombre = "Nuevo",
                Apellido = "Usuario",
                Correo = "nuevo.it@test.com",
                PasswordHash = "secreto",
                RolId = 3 // aunque se envíe otro, el servicio fuerza Cliente (1)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/usuarios", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var creado = await response.Content.ReadFromJsonAsync<RegistroDTO>();
            creado!.UsuariosId.Should().BeGreaterThan(0);
            creado.Correo.Should().Be("nuevo.it@test.com");
            creado.RolId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-USR-05: PUT /api/usuarios/{id}/rol con datos válidos devuelve 200")]
        public async Task CambiarRol_Valid_ReturnsOk()
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/usuarios/1/rol", new RolUpdateDTO { RolId = 2 });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<RegistroDTO>();
            actualizado!.RolId.Should().Be(2);
            actualizado.NombreRol.Should().Be("Barbero");
        }

        [Fact(DisplayName = "IT-USR-06: PUT /api/usuarios/{id}/rol con rol inexistente devuelve 400")]
        public async Task CambiarRol_InvalidRole_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/usuarios/1/rol", new RolUpdateDTO { RolId = 9999 });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-USR-07: PUT /api/usuarios/{id}/rol con usuario inexistente devuelve 400")]
        public async Task CambiarRol_InvalidUser_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/usuarios/9999/rol", new RolUpdateDTO { RolId = 1 });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
