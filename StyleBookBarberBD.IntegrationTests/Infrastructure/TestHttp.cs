namespace StyleBookBarberBD.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Utilidades para las pruebas de integración de endpoints.
    /// </summary>
    public static class TestHttp
    {
        /// <summary>
        /// Extrae el Id creado a partir de la cabecera Location de una respuesta 201 Created.
        /// </summary>
        public static int IdFromLocation(HttpResponseMessage response)
        {
            var location = response.Headers.Location
                ?? throw new InvalidOperationException("La respuesta no incluye la cabecera Location.");

            var lastSegment = location.Segments[^1].Trim('/');
            return int.Parse(lastSegment);
        }
    }
}
