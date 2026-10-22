// Models/Cita.cs
using System.ComponentModel.DataAnnotations;

namespace StyleBookBarberBD.Models
{
    public class Citas
    {
        [Key]
        public int CitasId { get; set; }
        public int ClienteId { get; set; }
        public int BarberosId { get; set; }
        public int ServiciosId { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public Usuarios? Cliente { get; set; }
        public Barberos? Barberos { get; set; }
        public Servicios? Servicios { get; set; }
    }
}

