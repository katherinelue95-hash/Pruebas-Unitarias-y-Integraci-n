using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class CitasEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CitasEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-CIT-01: GET /api/citas devuelve 200 con la lista de citas")]
        public async Task GetCitas_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/citas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var citas = await response.Content.ReadFromJsonAsync<List<CitasDTos>>();
            citas.Should().NotBeNull();
            citas!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-CIT-02: GET /api/citas/{id} existente devuelve 200 con nombres relacionados")]
        public async Task GetCitaById_Existing_ReturnsOkWithNames()
        {
            // Act
            var response = await _client.GetAsync("/api/citas/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cita = await response.Content.ReadFromJsonAsync<CitasDTos>();
            cita!.CitasId.Should().Be(1);
            cita.ClienteNombre.Should().Be("Carlos López");
            cita.ServicioNombre.Should().Be("Corte clásico");
        }

        [Fact(DisplayName = "IT-CIT-03: GET /api/citas/{id} inexistente devuelve 404")]
        public async Task GetCitaById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/citas/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-CIT-04: POST /api/citas válida devuelve 201 en estado Pendiente")]
        public async Task CreateCita_Valid_ReturnsCreatedPendiente()
        {
            // Arrange
            var dto = new CitasDTos
            {
                ClienteId = 1,
                BarberosId = 1,
                ServiciosId = 1,
                FechaHora = new DateTime(2026, 6, 1, 9, 0, 0),
                MontoTotal = 8000m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/citas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var creada = await response.Content.ReadFromJsonAsync<CitasDTos>();
            creada!.CitasId.Should().BeGreaterThan(0);
            creada.Estado.Should().Be("Pendiente");
        }

        [Fact(DisplayName = "IT-CIT-05: POST /api/citas con cliente inexistente devuelve 400")]
        public async Task CreateCita_UnknownCliente_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CitasDTos { ClienteId = 9999, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 6, 2, 9, 0, 0) };

            // Act
            var response = await _client.PostAsJsonAsync("/api/citas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-CIT-06: POST /api/citas con horario ocupado devuelve 400")]
        public async Task CreateCita_OccupiedSlot_ReturnsBadRequest()
        {
            // Arrange: mismo barbero y misma FechaHora que la cita sembrada (2026-05-01 10:00).
            var dto = new CitasDTos { ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 5, 1, 10, 0, 0) };

            // Act
            var response = await _client.PostAsJsonAsync("/api/citas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-CIT-07: PATCH /api/citas/{id}/estado con estado válido devuelve 200")]
        public async Task CambiarEstado_Valid_ReturnsOk()
        {
            // Arrange: crear una cita propia.
            var creada = await (await _client.PostAsJsonAsync("/api/citas",
                new CitasDTos { ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 6, 3, 9, 0, 0) }))
                .Content.ReadFromJsonAsync<CitasDTos>();

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/citas/{creada!.CitasId}/estado", new CambioEstadoDTO { Estado = "Confirmada" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizada = await response.Content.ReadFromJsonAsync<CitasDTos>();
            actualizada!.Estado.Should().Be("Confirmada");
        }

        [Fact(DisplayName = "IT-CIT-08: PATCH /api/citas/{id}/estado con estado inválido devuelve 400")]
        public async Task CambiarEstado_InvalidState_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PatchAsJsonAsync("/api/citas/1/estado", new CambioEstadoDTO { Estado = "Inexistente" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-CIT-09: PATCH /api/citas/{id}/estado con cita inexistente devuelve 400")]
        public async Task CambiarEstado_UnknownCita_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PatchAsJsonAsync("/api/citas/9999/estado", new CambioEstadoDTO { Estado = "Confirmada" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-CIT-10: DELETE /api/citas/{id} existente devuelve 204")]
        public async Task DeleteCita_Existing_ReturnsNoContent()
        {
            // Arrange
            var creada = await (await _client.PostAsJsonAsync("/api/citas",
                new CitasDTos { ClienteId = 1, BarberosId = 1, ServiciosId = 1, FechaHora = new DateTime(2026, 6, 4, 9, 0, 0) }))
                .Content.ReadFromJsonAsync<CitasDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/citas/{creada!.CitasId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-CIT-11: DELETE /api/citas/{id} inexistente devuelve 404")]
        public async Task DeleteCita_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/citas/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
