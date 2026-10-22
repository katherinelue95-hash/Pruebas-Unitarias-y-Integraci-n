using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class RolesServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public RolesServices(
            StyleBookBarberBDContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RolesDTos>> GetRolesAsync()
        {
            var roles = await _context.Roles.ToListAsync();

            return _mapper.Map<List<RolesDTos>>(roles);
        }

        public async Task<RolesDTos?> GetRolByIdAsync(int id)
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.RolId == id);

            return rol != null
                ? _mapper.Map<RolesDTos>(rol)
                : null;
        }

        public async Task<RolesDTos> CrearRolAsync(RolesDTos dto)
        {
            var rol = _mapper.Map<Roles>(dto);

            _context.Roles.Add(rol);

            await _context.SaveChangesAsync();

            return _mapper.Map<RolesDTos>(rol);
        }

        public async Task<RolesDTos?> ActualizarRolAsync(int id, RolesDTos dto)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
                return null;

            rol.NombreRol = dto.NombreRol;

            await _context.SaveChangesAsync();

            return _mapper.Map<RolesDTos>(rol);
        }

        public async Task<bool> EliminarRolAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
                return false;

            _context.Roles.Remove(rol);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}