using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class RolesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RolesEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-ROL-01: GET /api/roles devuelve 200 con la lista de roles")]
        public async Task GetRoles_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var roles = await response.Content.ReadFromJsonAsync<List<RolesDTos>>();
            roles.Should().NotBeNull();
            roles!.Should().HaveCountGreaterThanOrEqualTo(3);
        }

        [Fact(DisplayName = "IT-ROL-02: GET /api/roles/{id} existente devuelve 200")]
        public async Task GetRolById_Existing_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/roles/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var rol = await response.Content.ReadFromJsonAsync<RolesDTos>();
            rol!.RolesId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-ROL-03: GET /api/roles/{id} inexistente devuelve 404")]
        public async Task GetRolById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/roles/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-ROL-04: POST /api/roles devuelve 201 y la cabecera Location")]
        public async Task CreateRol_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new RolesDTos { NombreRol = "Rol IT" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/roles", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var creado = await response.Content.ReadFromJsonAsync<RolesDTos>();
            creado!.RolesId.Should().BeGreaterThan(0);
            creado.NombreRol.Should().Be("Rol IT");
        }

        [Fact(DisplayName = "IT-ROL-05: PUT /api/roles/{id} existente devuelve 200")]
        public async Task UpdateRol_Existing_ReturnsOk()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/roles",
                new RolesDTos { NombreRol = "Para actualizar" }))
                .Content.ReadFromJsonAsync<RolesDTos>();

            var dto = new RolesDTos { NombreRol = "Actualizado" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/roles/{creado!.RolesId}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<RolesDTos>();
            actualizado!.NombreRol.Should().Be("Actualizado");
        }

        [Fact(DisplayName = "IT-ROL-06: PUT /api/roles/{id} inexistente devuelve 404")]
        public async Task UpdateRol_NotFound_Returns404()
        {
            // Arrange
            var dto = new RolesDTos { NombreRol = "X" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/roles/9999", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-ROL-07: DELETE /api/roles/{id} existente devuelve 204")]
        public async Task DeleteRol_Existing_ReturnsNoContent()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/roles",
                new RolesDTos { NombreRol = "Para borrar" }))
                .Content.ReadFromJsonAsync<RolesDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/roles/{creado!.RolesId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-ROL-08: DELETE /api/roles/{id} inexistente devuelve 404")]
        public async Task DeleteRol_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/roles/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
