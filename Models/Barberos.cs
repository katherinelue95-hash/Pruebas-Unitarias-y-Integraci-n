// Models/Barbero.cs
namespace StyleBookBarberBD.Models
{
    public class Barberos
    {
        public int BarberosId { get; set; }
        public int UsuariosId { get; set; }
        public string Especialidad { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public decimal Calificacion { get; set; }
        public string EstadoDisp { get; set; } = "Disponible";
        public Usuarios? Usuario { get; set; }
    }
}

