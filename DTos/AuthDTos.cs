namespace StyleBookBarberBD.DTOs
{
    public class AuthDTos
    {
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class RegistroDTO
    {
        public int UsuariosId { get; set; }
        public int RolId { get; set; } = 1;
        public string? NombreRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
