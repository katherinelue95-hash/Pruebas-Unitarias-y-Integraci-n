using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Services;

namespace StyleBookBarberBD.EndPoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this IEndpointRouteBuilder routes)
        {
            // Obtener todas las categorías
            routes.MapGet("/api/categorias", async (CategoriasServices service) =>
            {
                var categorias = await service.GetCategoriasAsync();

                return Results.Ok(categorias);
            })
            .WithTags("Categorias");

            // Obtener categoría por Id
            routes.MapGet("/api/categorias/{id}", async (int id, CategoriasServices service) =>
            {
                var categoria = await service.GetCategoriaByIdAsync(id);

                return categoria != null
                    ? Results.Ok(categoria)
                    : Results.NotFound();
            })
            .WithTags("Categorias");

            // Crear categoría
            routes.MapPost("/api/categorias", async (CategoriasDTos dto, CategoriasServices service) =>
            {
                var categoria = await service.CrearCategoriaAsync(dto);

                return Results.Created(
                    $"/api/categorias/{categoria.CategoriasId}",
                    categoria);
            })
                .WithTags("Categorias");

            // Actualizar categoría
            routes.MapPut("/api/categorias/{id}", async (int id, CategoriasDTos dto, CategoriasServices service) =>
            {
                var categoria = await service.ActualizarCategoriaAsync(id, dto);

                return categoria != null
                    ? Results.Ok(categoria)
                    : Results.NotFound();
            })
                .WithTags("Categorias");

            // Eliminar categoría
            routes.MapDelete("/api/categorias/{id}", async (int id, CategoriasServices service) =>
            {
                var eliminado = await service.EliminarCategoriaAsync(id);

                return eliminado
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithTags("Categorias");
        }
    }
}