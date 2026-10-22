// Models/Usuario.cs
namespace StyleBookBarberBD.Models
{
    public class Usuarios
    {
        public int UsuariosId { get; set; }
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Roles? Rol { get; set; }
    }
}