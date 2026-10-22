using StyleBookBarberBD.Services;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.DTos;

namespace StyleBookBarberBD.EndPoints
{
    public static class UsuariosEndpoints
    {
        public static void MapUsuariosEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapGet("/api/usuarios", async (UsuariosServices service) =>
            {
                var usuarios = await service.GetUsuariosAsync();
                return Results.Ok(usuarios);
            })
            .WithTags("Usuarios");

            routes.MapGet("/api/usuarios/{id}", async (int id, UsuariosServices service) =>
            {
                var usuario = await service.GetUsuarioByIdAsync(id);
                return usuario != null ? Results.Ok(usuario) : Results.NotFound();
            })
            .WithTags("Usuarios");

            routes.MapPost("/api/usuarios", async (RegistroDTO dto, UsuariosServices service) =>
            {
                var nuevoUsuario = await service.CrearUsuarioAsync(dto);
                return Results.Created($"/api/usuarios/{nuevoUsuario.Correo}", nuevoUsuario);
            })
            .WithTags("Usuarios");

            routes.MapPut("/api/usuarios/{id}/rol", async (int id, RolUpdateDTO dto, UsuariosServices service) =>
            {
                var actualizado = await service.CambiarRolAsync(id, dto.RolId);
                return actualizado != null
                    ? Results.Ok(actualizado)
                    : Results.BadRequest(new { mensaje = "No se pudo cambiar el rol: usuario o rol no válidos." });
            })
            .WithTags("Usuarios");
        }
    }
}

