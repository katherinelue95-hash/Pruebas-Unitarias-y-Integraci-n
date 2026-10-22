using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;
using AutoMapper;

namespace StyleBookBarberBD.Services
{
    public class UsuariosServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public UsuariosServices(StyleBookBarberBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RegistroDTO>> GetUsuariosAsync()
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
            return _mapper.Map<List<RegistroDTO>>(usuarios);
        }

        public async Task<RegistroDTO?> GetUsuarioByIdAsync(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol)
                                                 .FirstOrDefaultAsync(u => u.UsuariosId == id);
            return usuario != null ? _mapper.Map<RegistroDTO>(usuario) : null;
        }

        public async Task<RegistroDTO> CrearUsuarioAsync(RegistroDTO dto)
        {
            var usuario = _mapper.Map<Usuarios>(dto);
            usuario.RolId = 1; // Por defecto, cliente
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return _mapper.Map<RegistroDTO>(usuario);
        }

        public async Task<RegistroDTO?> CambiarRolAsync(int id, int rolId)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol)
                                                 .FirstOrDefaultAsync(u => u.UsuariosId == id);
            if (usuario is null)
                return null;

            var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == rolId);
            if (!rolExiste)
                return null;

            usuario.RolId = rolId;
            await _context.SaveChangesAsync();

            var resultado = _mapper.Map<RegistroDTO>(usuario);
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.RolId == rolId);
            resultado.NombreRol = rol?.NombreRol;
            return resultado;
        }
    }
}
