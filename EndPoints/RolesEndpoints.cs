using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Services;

namespace StyleBookBarberBD.EndPoints
{
    public static class RolesEndpoints
    {
        public static void MapRolesEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapGet("/api/roles", async (RolesServices service) =>
            {
                var roles = await service.GetRolesAsync();

                return Results.Ok(roles);
            })
            .WithTags("Roles");

            routes.MapGet("/api/roles/{id}", async (int id, RolesServices service) =>
            {
                var rol = await service.GetRolByIdAsync(id);

                return rol != null
                    ? Results.Ok(rol)
                    : Results.NotFound();
            })
            .WithTags("Roles");

            routes.MapPost("/api/roles", async (RolesDTos dto, RolesServices service) =>
            {
                var rol = await service.CrearRolAsync(dto);

                return Results.Created(
                    $"/api/roles/{rol.RolesId}",
                    rol);
            })
            .WithTags("Roles");

            routes.MapPut("/api/roles/{id}", async (int id, RolesDTos dto, RolesServices service) =>
            {
                var rol = await service.ActualizarRolAsync(id, dto);

                return rol != null
                    ? Results.Ok(rol)
                    : Results.NotFound();
            })
            .WithTags("Roles");

            routes.MapDelete("/api/roles/{id}", async (int id, RolesServices service) =>
            {
                var eliminado = await service.EliminarRolAsync(id);

                return eliminado
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithTags("Roles");
        }
    }
}