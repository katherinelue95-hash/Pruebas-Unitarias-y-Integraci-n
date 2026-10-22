// Models/Rol.cs
using System.ComponentModel.DataAnnotations;

namespace StyleBookBarberBD.Models
{
    public class Roles
    {
        [Key]
        public int RolId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}
