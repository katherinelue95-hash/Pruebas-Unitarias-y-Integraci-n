using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class HorariosBarberoEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HorariosBarberoEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-HOR-01: GET /api/horariosbarbero devuelve 200 con la lista de horarios")]
        public async Task GetHorarios_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/horariosbarbero");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var horarios = await response.Content.ReadFromJsonAsync<List<HorariosBarberoDTos>>();
            horarios.Should().NotBeNull();
            horarios!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-HOR-02: GET /api/horariosbarbero/{id} existente devuelve 200")]
        public async Task GetHorarioById_Existing_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/horariosbarbero/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var horario = await response.Content.ReadFromJsonAsync<HorariosBarberoDTos>();
            horario!.HorariosId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-HOR-03: GET /api/horariosbarbero/{id} inexistente devuelve 404")]
        public async Task GetHorarioById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/horariosbarbero/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-HOR-04: POST /api/horariosbarbero devuelve 201 y la cabecera Location")]
        public async Task CreateHorario_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 2, HoraInicio = "09:00", HoraFin = "18:00" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/horariosbarbero", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var creado = await response.Content.ReadFromJsonAsync<HorariosBarberoDTos>();
            creado!.HorariosId.Should().BeGreaterThan(0);
            creado.HoraInicio.Should().Be("09:00");
        }

        [Fact(DisplayName = "IT-HOR-05: PUT /api/horariosbarbero/{id} existente devuelve 200")]
        public async Task UpdateHorario_Existing_ReturnsOk()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/horariosbarbero",
                new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 3, HoraInicio = "08:00", HoraFin = "12:00" }))
                .Content.ReadFromJsonAsync<HorariosBarberoDTos>();

            var dto = new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 3, HoraInicio = "10:00", HoraFin = "14:00" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/horariosbarbero/{creado!.HorariosId}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizado = await response.Content.ReadFromJsonAsync<HorariosBarberoDTos>();
            actualizado!.HoraInicio.Should().Be("10:00");
            actualizado.HoraFin.Should().Be("14:00");
        }

        [Fact(DisplayName = "IT-HOR-06: PUT /api/horariosbarbero/{id} inexistente devuelve 404")]
        public async Task UpdateHorario_NotFound_Returns404()
        {
            // Arrange
            var dto = new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 1, HoraInicio = "08:00", HoraFin = "09:00" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/horariosbarbero/9999", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-HOR-07: DELETE /api/horariosbarbero/{id} existente devuelve 204")]
        public async Task DeleteHorario_Existing_ReturnsNoContent()
        {
            // Arrange
            var creado = await (await _client.PostAsJsonAsync("/api/horariosbarbero",
                new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 5, HoraInicio = "08:00", HoraFin = "09:00" }))
                .Content.ReadFromJsonAsync<HorariosBarberoDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/horariosbarbero/{creado!.HorariosId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-HOR-08: DELETE /api/horariosbarbero/{id} inexistente devuelve 404")]
        public async Task DeleteHorario_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/horariosbarbero/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
