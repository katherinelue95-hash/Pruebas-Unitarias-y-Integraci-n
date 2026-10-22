namespace StyleBookBarberBD.DTos
{
    public class ResenasDTos
    {
        public int ResenaId { get; set; }
        public int BarberosId { get; set; }
        public int UsuariosId { get; set; }
        public int Estrellas { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Autor { get; set; } = string.Empty;
        public string BarberoNombre { get; set; } = string.Empty;

        // Calificación promedio recalculada del barbero (se devuelve al crear/consultar)
        public decimal CalificacionPromedio { get; set; }
    }
}