using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.IntegrationTests.Infrastructure;
using Xunit;

namespace StyleBookBarberBD.IntegrationTests.Endpoints
{
    public class CategoriasEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CategoriasEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "IT-CAT-01: GET /api/categorias devuelve 200 con la lista de categorías")]
        public async Task GetCategorias_ReturnsOkWithList()
        {
            // Act
            var response = await _client.GetAsync("/api/categorias");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var categorias = await response.Content.ReadFromJsonAsync<List<CategoriasDTos>>();
            categorias.Should().NotBeNull();
            categorias!.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "IT-CAT-02: GET /api/categorias/{id} existente devuelve 200")]
        public async Task GetCategoriaById_Existing_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/categorias/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var categoria = await response.Content.ReadFromJsonAsync<CategoriasDTos>();
            categoria!.CategoriasId.Should().Be(1);
        }

        [Fact(DisplayName = "IT-CAT-03: GET /api/categorias/{id} inexistente devuelve 404")]
        public async Task GetCategoriaById_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/categorias/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-CAT-04: POST /api/categorias devuelve 201 y la cabecera Location")]
        public async Task CreateCategoria_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new CategoriasDTos { NombreCategoria = "Categoría IT", Descripcion = "Creada en prueba" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/categorias", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var creada = await response.Content.ReadFromJsonAsync<CategoriasDTos>();
            creada!.CategoriasId.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "IT-CAT-05: PUT /api/categorias/{id} existente devuelve 200")]
        public async Task UpdateCategoria_Existing_ReturnsOk()
        {
            // Arrange
            var creada = await (await _client.PostAsJsonAsync("/api/categorias",
                new CategoriasDTos { NombreCategoria = "Para actualizar", Descripcion = "x" }))
                .Content.ReadFromJsonAsync<CategoriasDTos>();

            var dto = new CategoriasDTos { NombreCategoria = "Actualizada", Descripcion = "y" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/categorias/{creada!.CategoriasId}", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualizada = await response.Content.ReadFromJsonAsync<CategoriasDTos>();
            actualizada!.NombreCategoria.Should().Be("Actualizada");
        }

        [Fact(DisplayName = "IT-CAT-06: PUT /api/categorias/{id} inexistente devuelve 404")]
        public async Task UpdateCategoria_NotFound_Returns404()
        {
            // Arrange
            var dto = new CategoriasDTos { NombreCategoria = "X" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/categorias/9999", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "IT-CAT-07: DELETE /api/categorias/{id} existente devuelve 204")]
        public async Task DeleteCategoria_Existing_ReturnsNoContent()
        {
            // Arrange
            var creada = await (await _client.PostAsJsonAsync("/api/categorias",
                new CategoriasDTos { NombreCategoria = "Para borrar", Descripcion = "temporal" }))
                .Content.ReadFromJsonAsync<CategoriasDTos>();

            // Act
            var response = await _client.DeleteAsync($"/api/categorias/{creada!.CategoriasId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "IT-CAT-08: DELETE /api/categorias/{id} inexistente devuelve 404")]
        public async Task DeleteCategoria_NotFound_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/categorias/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
