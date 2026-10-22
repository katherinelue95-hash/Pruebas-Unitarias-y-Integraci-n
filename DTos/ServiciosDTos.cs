namespace StyleBookBarberBD.DTos
{
    public class ServiciosDTos
    {
        public int ServiciosId { get; set; }
        public int CategoriaId { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMin { get; set; }
        public string? FotoUrl { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
    }
}
