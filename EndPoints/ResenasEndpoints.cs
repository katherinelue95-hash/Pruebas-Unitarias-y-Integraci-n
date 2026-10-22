using StyleBookBarberBD.Services;
using StyleBookBarberBD.DTos;

namespace StyleBookBarberBD.EndPoints
{
    public static class ResenasEndpoints
    {
        public static void MapResenasEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapGet("/api/resenas", async (ResenasServices service) =>
            {
                var resenas = await service.GetResenasAsync();
                return Results.Ok(resenas);
            })
            .WithTags("Resenas");

            routes.MapGet("/api/resenas/barbero/{barberoId}", async (int barberoId, ResenasServices service) =>
            {
                var resenas = await service.GetResenasByBarberoAsync(barberoId);
                return Results.Ok(resenas);
            })
            .WithTags("Resenas");

            routes.MapPost("/api/resenas", async (ResenasDTos dto, ResenasServices service) =>
            {
                var nueva = await service.CrearResenaAsync(dto);
                return nueva != null
                    ? Results.Created($"/api/resenas/{nueva.ResenaId}", nueva)
                    : Results.BadRequest(new { mensaje = "Usuario o barbero no válido, o calificación fuera de rango (1-5)." });
            })
            .WithTags("Resenas");

            routes.MapDelete("/api/resenas/{id}", async (int id, ResenasServices service) =>
            {
                var eliminada = await service.EliminarResenaAsync(id);
                return eliminada ? Results.NoContent() : Results.NotFound();
            })
            .WithTags("Resenas");
        }
    }
}