// Models/HorarioBarbero.cs
using System.ComponentModel.DataAnnotations;

namespace StyleBookBarberBD.Models
{
    public class HorariosBarbero
    {
        [Key]
        public int HorariosId { get; set; }
        public int BarberosId { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public Barberos? Barbero { get; set; }
    }
}
