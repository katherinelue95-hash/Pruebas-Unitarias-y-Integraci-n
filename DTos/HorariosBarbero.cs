namespace StyleBookBarberBD.DTos
{
    public class HorariosBarberoDTos
    {
        public int HorariosId { get; set; }
        public int BarberoId { get; set; }
        public int DiaSemana { get; set; }   // 1=Lunes, 2=Martes, etc.
        public string HoraInicio { get; set; } = string.Empty; // formato "08:00"
        public string HoraFin { get; set; } = string.Empty;    // formato "17:00"
    }
}
