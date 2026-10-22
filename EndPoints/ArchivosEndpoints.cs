namespace StyleBookBarberBD.EndPoints
{
    public static class ArchivosEndpoints
    {
        public static void MapArchivosEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/api/archivos/upload", async (IFormFile file, HttpRequest request) =>
            {
                if (file is null || file.Length == 0)
                    return Results.BadRequest(new { mensaje = "No se recibió ningún archivo." });

                string extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(extension))
                    extension = ".jpg";

                var nombre = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(carpeta);

                var ruta = Path.Combine(carpeta, nombre);
                using (var stream = File.Create(ruta))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"{request.Scheme}://{request.Host}/uploads/{nombre}";
                return Results.Ok(new { url });
            })
            .WithTags("Archivos")
            .DisableAntiforgery();
        }
    }
}