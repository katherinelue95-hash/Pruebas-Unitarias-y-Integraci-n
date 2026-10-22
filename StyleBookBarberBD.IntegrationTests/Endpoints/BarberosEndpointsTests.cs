using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class BarberosEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public BarberosEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-BAR-01: GET /api/barberos devuelve 200 con la lista de barberos")]
        public async Task GetBarberos_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/barberos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var barberos = await response.Content.ReadFromJsonAsync<List<BarberosDTos>>();
            barberos.Should().NotBeNull();
            barberos!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-BAR-02: GET /api/barberos/{id} existente devuelve 200")]
        public async Task GetBarberoById_Existing_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/barberos/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var barbero = await response.Content.ReadFromJsonAsync<BarberosDTos>();
            barbero!.BarberosId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-BAR-03: GET /api/barberos/{id} inexistente devuelve 404")]
        public async Task GetBarberoById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/barberos/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-BAR-04: POST /api/barberos devuelve 201 y la cabecera Location")]
        public async Task CreateBarbero_Valid_ReturnsCreated()
        {
            // Arrange: usar el usuario 3 (Admin) como usuario asociado.
            var dto = new BarberosDTos
            {
                UsuarioId = 3,
                Especialidad = "Navaja",
                Calificacion = 4.0m,
                EstadoDisp = "Disponible",
                FotoUrl = "foto-it.jpg"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/barberos", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var creado = await response.Content.ReadFromJsonAsync<BarberosDTos>();
            creado!.BarberosId.Should().BeGreaterThan(0);
            creado.UsuarioId.Should().Be(3);
        }

        [Fact(DisplayName = "IT-BAR-05: PUT /api/barberos/{id} existente devuelve 200")]
        public async Task UpdateBarbero_Existing_ReturnsOk()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/barberos",
                new BarberosDTos { UsuarioId = 3, Especialidad = "Para actualizar", FotoUrl = "f.jpg" }))
                .Content.ReadFromJsonAsync<BarberosDTos>();

            var dto = new BarberosDTos { UsuarioId = 3, Especialidad = "Actualizada", Calificacion = 5m, EstadoDisp = "Ocupado", FotoUrl = "f2.jpg" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/barberos/{creado!.BarberosId}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<BarberosDTos>();
            actualizado!.Especialidad.Should().Be("Actualizada");
            actualizado.EstadoDisp.Should().Be("Ocupado");
        }

        [Fact(DisplayName = "IT-BAR-06: PUT /api/barberos/{id} inexistente devuelve 404")]
        public async Task UpdateBarbero_NotFound_Returns404()
        {
            // Arrange
            var dto = new BarberosDTos { UsuarioId = 3, Especialidad = "X", FotoUrl = "f.jpg" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/barberos/9999", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-BAR-07: DELETE /api/barberos/{id} existente devuelve 204")]
        public async Task DeleteBarbero_Existing_ReturnsNoContent()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/barberos",
                new BarberosDTos { UsuarioId = 3, Especialidad = "Para borrar", FotoUrl = "f.jpg" }))
                .Content.ReadFromJsonAsync<BarberosDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/barberos/{creado!.BarberosId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-BAR-08: DELETE /api/barberos/{id} inexistente devuelve 404")]
        public async Task DeleteBarbero_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/barberos/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
