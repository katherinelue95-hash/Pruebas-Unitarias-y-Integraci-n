using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Services;

namespace StyleBookBarberBD.EndPoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/api/auth/login",
                async (AuthDTos dto, AuthServices service) =>
                {
                    var usuario = await service.LoginAsync(dto);

                    if (usuario == null)
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(new
                    {
                        usuario.UsuariosId,
                        usuario.Nombre,
                        usuario.Apellido,
                        usuario.Correo,
                        usuario.RolId
                    });
                })
                .WithTags("Auth");
        }
    }
}