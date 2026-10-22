using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class ServiciosEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ServiciosEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-SRV-01: GET /api/servicios devuelve 200 con la lista de servicios")]
        public async Task GetServicios_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/servicios");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var servicios = await response.Content.ReadFromJsonAsync<List<ServiciosDTos>>();
            servicios.Should().NotBeNull();
            servicios!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-SRV-02: GET /api/servicios/{id} existente devuelve 200")]
        public async Task GetServicioById_Existing_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/servicios/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var servicio = await response.Content.ReadFromJsonAsync<ServiciosDTos>();
            servicio!.ServiciosId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-SRV-03: GET /api/servicios/{id} inexistente devuelve 404")]
        public async Task GetServicioById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/servicios/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-SRV-04: POST /api/servicios devuelve 201 y la cabecera Location")]
        public async Task CreateServicio_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new ServiciosDTos { CategoriaId = 1, NombreServicio = "Corte IT", Precio = 9000m, DuracionMin = 40 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/servicios", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var creado = await response.Content.ReadFromJsonAsync<ServiciosDTos>();
            creado!.ServiciosId.Should().BeGreaterThan(0);
            creado.NombreServicio.Should().Be("Corte IT");
        }

        [Fact(DisplayName = "IT-SRV-05: PUT /api/servicios/{id} existente devuelve 200 con los datos actualizados")]
        public async Task UpdateServicio_Existing_ReturnsOk()
        {
            // Arrange: crear uno propio para no afectar los datos sembrados.
            var creado = await (await _client.PostAsJsonAsync("/api/servicios",
                new ServiciosDTos { CategoriaId = 1, NombreServicio = "Para actualizar", Precio = 1000m, DuracionMin = 10 }))
                .Content.ReadFromJsonAsync<ServiciosDTos>();
            var id = creado!.ServiciosId;

            var dto = new ServiciosDTos { CategoriaId = 2, NombreServicio = "Actualizado", Precio = 2000m, DuracionMin = 15 };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/servicios/{id}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<ServiciosDTos>();
            actualizado!.NombreServicio.Should().Be("Actualizado");
            actualizado.Precio.Should().Be(2000m);
        }

        [Fact(DisplayName = "IT-SRV-06: PUT /api/servicios/{id} inexistente devuelve 404")]
        public async Task UpdateServicio_NotFound_Returns404()
        {
            // Arrange
            var dto = new ServiciosDTos { CategoriaId = 1, NombreServicio = "X", Precio = 1m, DuracionMin = 1 };

            // Act
            var response = await _client.PutAsJsonAsync("/api/servicios/9999", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-SRV-07: DELETE /api/servicios/{id} existente devuelve 204")]
        public async Task DeleteServicio_Existing_ReturnsNoContent()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/servicios",
                new ServiciosDTos { CategoriaId = 1, NombreServicio = "Para borrar", Precio = 1m, DuracionMin = 1 }))
                .Content.ReadFromJsonAsync<ServiciosDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/servicios/{creado!.ServiciosId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-SRV-08: DELETE /api/servicios/{id} inexistente devuelve 404")]
        public async Task DeleteServicio_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/servicios/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
