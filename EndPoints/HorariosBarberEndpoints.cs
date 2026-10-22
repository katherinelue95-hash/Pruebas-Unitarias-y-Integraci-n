using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Services;

namespace StyleBookBarberBD.EndPoints
{
    public static class HorariosBareroEndpoints
    {
        public static void MapHorariosBareroEndpoints(this IEndpointRouteBuilder routes)
        {
            // Obtener todos los horarios
            routes.MapGet("/api/horariosbarbero", async (HorariosBarberoServices service) =>
            {
                var horarios = await service.GetHorariosAsync();
                return Results.Ok(horarios);
            })
            .WithTags("HorariosBarbero");

            // Obtener horario por Id
            routes.MapGet("/api/horariosbarbero/{id}", async (int id, HorariosBarberoServices service) =>
            {
                var horario = await service.GetHorarioByIdAsync(id);

                return horario != null
                    ? Results.Ok(horario)
                    : Results.NotFound();
            })
            .WithTags("HorariosBarbero");

            // Crear horario
            routes.MapPost("/api/horariosbarbero", async (HorariosBarberoDTos dto, HorariosBarberoServices service) =>
            {
                var horario = await service.CrearHorarioAsync(dto);

                return Results.Created(
                    $"/api/horariosbarbero/{horario.HorariosId}",
                    horario);
            })
            .WithTags("HorariosBarbero");

            // Actualizar horario
            routes.MapPut("/api/horariosbarbero/{id}", async (int id, HorariosBarberoDTos dto, HorariosBarberoServices service) =>
            {
                var horario = await service.ActualizarHorarioAsync(id, dto);

                return horario != null
                    ? Results.Ok(horario)
                    : Results.NotFound();
            })
            .WithTags("HorariosBarbero");

            // Eliminar horario
            routes.MapDelete("/api/horariosbarbero/{id}", async (int id, HorariosBarberoServices service) =>
            {
                var eliminado = await service.EliminarHorarioAsync(id);

                return eliminado
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithTags("HorariosBarbero");
        }
    }
}

