namespace StyleBookBarberBD.DTos
{
    public class BarberosDTos
    {
        public int BarberosId { get; set; }
        public decimal Calificacion { get; set; }
        public string EstadoDisp { get; set; } = "Disponible";
        public string? FotoUrl { get; set; }
        public string Especialidad { get; set; }

        // Relación con Usuario (solo datos básicos)
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }
}
