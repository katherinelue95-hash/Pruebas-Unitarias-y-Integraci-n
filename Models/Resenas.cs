// Models/Resenas.cs
using System.ComponentModel.DataAnnotations;

namespace StyleBookBarberBD.Models
{
    public class Resenas
    {
        [Key]
        public int ResenaId { get; set; }
        public int BarberosId { get; set; }
        public int UsuariosId { get; set; }
        public int Estrellas { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public Barberos? Barberos { get; set; }
        public Usuarios? Usuario { get; set; }
    }
}