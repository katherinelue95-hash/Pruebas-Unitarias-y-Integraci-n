using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class ServiciosServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public ServiciosServices(StyleBookBarberBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 🔹 Obtener todos los servicios
        public async Task<List<ServiciosDTos>> GetServiciosAsync()
        {
            var servicios = await _context.Servicios.Include(s => s.Categoria).ToListAsync();
            return _mapper.Map<List<ServiciosDTos>>(servicios);
        }

        // 🔹 Obtener servicio por Id
        public async Task<ServiciosDTos?> GetServicioByIdAsync(int id)
        {
            var servicio = await _context.Servicios.Include(s => s.Categoria)
                                                   .FirstOrDefaultAsync(s => s.ServiciosId == id);
            return servicio != null ? _mapper.Map<ServiciosDTos>(servicio) : null;
        }

        // 🔹 Crear servicio
        public async Task<ServiciosDTos> CrearServicioAsync(ServiciosDTos dto)
        {
            var servicio = _mapper.Map<Servicios>(dto);
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
            return _mapper.Map<ServiciosDTos>(servicio);
        }

        // 🔹 Actualizar servicio
        public async Task<ServiciosDTos?> ActualizarServicioAsync(int id, ServiciosDTos dto)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return null;

            servicio.CategoriaId = dto.CategoriaId;
            servicio.NombreServicio = dto.NombreServicio;
            servicio.Precio = dto.Precio;
            servicio.DuracionMin = dto.DuracionMin;
            servicio.FotoUrl = dto.FotoUrl;

            await _context.SaveChangesAsync();

            return _mapper.Map<ServiciosDTos>(servicio);
        }

        // 🔹 Eliminar servicio
        public async Task<bool> EliminarServicioAsync(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return false;

            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

