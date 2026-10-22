using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Services;

namespace StyleBookBarberBD.EndPoints
{
    public static class BarberosEndpoints
    {
        public static void MapBarberosEndpoints(this IEndpointRouteBuilder routes)
        {
            // 🔹 Obtener todos los barberos
            routes.MapGet("/api/barberos", async (BarberosServices service) =>
            {
                var barberos = await service.GetBarberosAsync();
                return Results.Ok(barberos);
            })
            .WithTags("Barberos");

            // 🔹 Obtener un barbero por Id
            routes.MapGet("/api/barberos/{id}", async (int id, BarberosServices service) =>
            {
                var barbero = await service.GetBarberoByIdAsync(id);
                return barbero != null ? Results.Ok(barbero) : Results.NotFound();
            })
            .WithTags("Barberos");

            // 🔹 Crear un nuevo barbero
            routes.MapPost("/api/barberos", async (BarberosDTos dto, BarberosServices service) =>
            {
                var nuevoBarbero = await service.CrearBarberoAsync(dto);
                return Results.Created($"/api/barberos/{nuevoBarbero.BarberosId}", nuevoBarbero);
            })
            .WithTags("Barberos");

            // 🔹 Actualizar un barbero
            routes.MapPut("/api/barberos/{id}", async (int id, BarberosDTos dto, BarberosServices service) =>
            {
                var actualizado = await service.ActualizarBarberoAsync(id, dto);
                return actualizado != null ? Results.Ok(actualizado) : Results.NotFound();
            })
            .WithTags("Barberos");

            // 🔹 Eliminar un barbero
            routes.MapDelete("/api/barberos/{id}", async (int id, BarberosServices service) =>
            {
                var eliminado = await service.EliminarBarberoAsync(id);
                return eliminado ? Results.NoContent() : Results.NotFound();
            })
            .WithTags("Barberos");
        }
    }
}


