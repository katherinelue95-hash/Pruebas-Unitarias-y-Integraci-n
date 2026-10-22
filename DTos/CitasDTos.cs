namespace StyleBookBarberBD.DTos
{
    public class CitasDTos
    {

        public int CitasId { get; set; }
        public int ClienteId { get; set; }
        public int BarberosId { get; set; }
        public int ServiciosId { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string ClienteNombre { get; set; } = string.Empty;
        public string BarberoNombre { get; set; } = string.Empty;
        public string ServicioNombre { get; set; } = string.Empty;
    }
}
