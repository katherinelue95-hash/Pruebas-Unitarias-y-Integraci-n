using StyleBookBarberBD.Services;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.DTos;

namespace StyleBookBarberBD.EndPoints
{
    public static class CitasEndpoints
    {
        public static void MapCitasEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapGet("/api/citas", async (CitasServices service) =>
            {
                var citas = await service.GetCitasAsync();
                return Results.Ok(citas);
            })
            .WithTags("Citas");

            routes.MapGet("/api/citas/{id}", async (int id, CitasServices service) =>
            {
                var cita = await service.GetCitaByIdAsync(id);
                return cita != null ? Results.Ok(cita) : Results.NotFound();
            })
            .WithTags("Citas");

            routes.MapPost("/api/citas", async (CitasDTos dto, CitasServices service) =>
            {
                var nuevaCita = await service.CrearCitaAsync(dto);
                return nuevaCita != null
                    ? Results.Created($"/api/citas/{nuevaCita.CitasId}", nuevaCita)
                    : Results.BadRequest(new { mensaje = "No se pudo registrar la cita: verifica el usuario, barbero y servicio, o elige otro horario." });
            })
            .WithTags("Citas");

            routes.MapPatch("/api/citas/{id}/estado", async (int id, CambioEstadoDTO dto, CitasServices service) =>
            {
                if (string.IsNullOrWhiteSpace(dto.Estado))
                    return Results.BadRequest(new { mensaje = "Indica un estado válido." });

                var actualizada = await service.CambiarEstadoAsync(id, dto.Estado);
                return actualizada != null
                    ? Results.Ok(actualizada)
                    : Results.BadRequest(new { mensaje = "No se pudo cambiar el estado: el estado debe ser Pendiente, Confirmada, En curso, Completada o Cancelada." });
            })
            .WithTags("Citas");

            routes.MapDelete("/api/citas/{id}", async (int id, CitasServices service) =>
            {
                var eliminada = await service.EliminarCitaAsync(id);
                return eliminada ? Results.NoContent() : Results.NotFound();
            })
            .WithTags("Citas");
        }
    }
}

