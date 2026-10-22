using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class AuthServices
    {
        private readonly StyleBookBarberBDContext _context;

        public AuthServices(StyleBookBarberBDContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> LoginAsync(AuthDTos dto)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo == dto.Correo &&
                    u.PasswordHash == dto.PasswordHash);
        }
    }
}
