// Models/Categoria.cs
namespace StyleBookBarberBD.Models
{
    public class Categorias
    {
        public int CategoriasId { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}