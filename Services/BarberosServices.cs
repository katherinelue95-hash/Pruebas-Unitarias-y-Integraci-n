using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class BarberosServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public BarberosServices(StyleBookBarberBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 🔹 Obtener todos los barberos
        public async Task<List<BarberosDTos>> GetBarberosAsync()
        {
            var barberos = await _context.Barberos.Include(b => b.Usuario).ToListAsync();
            return _mapper.Map<List<BarberosDTos>>(barberos);
        }

        // 🔹 Obtener barbero por Id
        public async Task<BarberosDTos?> GetBarberoByIdAsync(int id)
        {
            var barbero = await _context.Barberos.Include(b => b.Usuario)
                                                 .FirstOrDefaultAsync(b => b.BarberosId == id);
            return barbero != null ? _mapper.Map<BarberosDTos>(barbero) : null;
        }

        // 🔹 Crear barbero
        public async Task<BarberosDTos> CrearBarberoAsync(BarberosDTos dto)
        {
            var barbero = _mapper.Map<Barberos>(dto);
            _context.Barberos.Add(barbero);
            await _context.SaveChangesAsync();
            return _mapper.Map<BarberosDTos>(barbero);
        }

        // 🔹 Actualizar barbero
        public async Task<BarberosDTos?> ActualizarBarberoAsync(int id, BarberosDTos dto)
        {
            var barbero = await _context.Barberos.FindAsync(id);

            if (barbero == null)
                return null;

            // No modificar la llave primaria
            barbero.UsuariosId = dto.UsuarioId;
            barbero.Especialidad = dto.Especialidad;
            barbero.FotoUrl = dto.FotoUrl;
            barbero.Calificacion = dto.Calificacion;
            barbero.EstadoDisp = dto.EstadoDisp;

            await _context.SaveChangesAsync();

            return _mapper.Map<BarberosDTos>(barbero);
        }

        // 🔹 Eliminar barbero
        public async Task<bool> EliminarBarberoAsync(int id)
        {
            var barbero = await _context.Barberos.FindAsync(id);
            if (barbero == null) return false;

            _context.Barberos.Remove(barbero);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
