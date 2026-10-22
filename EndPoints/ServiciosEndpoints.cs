using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;

namespace StyleBookBarberBD.EndPoints
{
    public static class ServiciosEndpoints
    {
        public static void MapServiciosEndpoints(this IEndpointRouteBuilder routes)
        {
            // 🔹 Obtener todos los servicios
            routes.MapGet("/api/servicios", async (ServiciosServices service) =>
            {
                var servicios = await service.GetServiciosAsync();
                return Results.Ok(servicios);
            })
            .WithTags("Servicios");

            // 🔹 Obtener un servicio por Id
            routes.MapGet("/api/servicios/{id}", async (int id, ServiciosServices service) =>
            {
                var servicio = await service.GetServicioByIdAsync(id);
                return servicio != null ? Results.Ok(servicio) : Results.NotFound();
            })
                .WithTags("Servicios");

            // 🔹 Crear un nuevo servicio
            routes.MapPost("/api/servicios", async (ServiciosDTos dto, ServiciosServices service) =>
            {
                var nuevoServicio = await service.CrearServicioAsync(dto);
                return Results.Created($"/api/servicios/{nuevoServicio.ServiciosId}", nuevoServicio);
            })
            .WithTags("Servicios");

            // 🔹 Actualizar un servicio existente
            routes.MapPut("/api/servicios/{id}", async (int id, ServiciosDTos dto, ServiciosServices service) =>
            {
                var actualizado = await service.ActualizarServicioAsync(id, dto);
                return actualizado != null ? Results.Ok(actualizado) : Results.NotFound();
            })
            .WithTags("Servicios");

            // 🔹 Eliminar un servicio
            routes.MapDelete("/api/servicios/{id}", async (int id, ServiciosServices service) =>
            {
                var eliminado = await service.EliminarServicioAsync(id);
                return eliminado ? Results.NoContent() : Results.NotFound();
            })
            .WithTags("Servicios");
        }
    }
}


