using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class ResenasEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ResenasEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-RES-01: GET /api/resenas devuelve 200 con la lista de reseñas")]
        public async Task GetResenas_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/resenas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resenas = await response.Content.ReadFromJsonAsync<List<ResenasDTos>>();
            resenas.Should().NotBeNull();
            resenas!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-RES-02: GET /api/resenas/barbero/{id} devuelve 200 con las reseñas del barbero")]
        public async Task GetResenasByBarbero_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/resenas/barbero/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resenas = await response.Content.ReadFromJsonAsync<List<ResenasDTos>>();
            resenas.Should().NotBeNull();
            resenas!.Should().OnlyContain(r => r.BarberosId == 1);
        }

        [Fact(DisplayName = "IT-RES-03: POST /api/resenas válida devuelve 201 y recalcula el promedio")]
        public async Task CreateResena_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new ResenasDTos { BarberosId = 1, UsuariosId = 1, Estrellas = 4, Comentario = "Muy bueno" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/resenas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var creada = await response.Content.ReadFromJsonAsync<ResenasDTos>();
            creada!.ResenaId.Should().BeGreaterThan(0);
            creada.CalificacionPromedio.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "IT-RES-04: POST /api/resenas con estrellas fuera de rango devuelve 400")]
        public async Task CreateResena_InvalidStars_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResenasDTos { BarberosId = 1, UsuariosId = 1, Estrellas = 9, Comentario = "Inválida" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/resenas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-RES-05: POST /api/resenas con barbero inexistente devuelve 400")]
        public async Task CreateResena_UnknownBarbero_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResenasDTos { BarberosId = 9999, UsuariosId = 1, Estrellas = 5, Comentario = "Inválida" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/resenas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "IT-RES-06: DELETE /api/resenas/{id} existente devuelve 204")]
        public async Task DeleteResena_Existing_ReturnsNoContent()
        {
            // Arrange
            var creada = await (await _client.PostAsJsonAsync("/api/resenas",
                new ResenasDTos { BarberosId = 1, UsuariosId = 1, Estrellas = 3, Comentario = "Para borrar" }))
                .Content.ReadFromJsonAsync<ResenasDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/resenas/{creada!.ResenaId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-RES-07: DELETE /api/resenas/{id} inexistente devuelve 404")]
        public async Task DeleteResena_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/resenas/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
