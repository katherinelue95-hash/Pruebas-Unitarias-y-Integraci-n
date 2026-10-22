using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class CitasServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public CitasServices(StyleBookBarberBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CitasDTos>> GetCitasAsync()
        {
            var citas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Barberos)!.ThenInclude(b => b.Usuario)
                .Include(c => c.Servicios)
                .ToListAsync();

            return _mapper.Map<List<CitasDTos>>(citas);
        }

        public async Task<CitasDTos?> GetCitaByIdAsync(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Barberos)!.ThenInclude(b => b.Usuario)
                .Include(c => c.Servicios)
                .FirstOrDefaultAsync(c => c.CitasId == id);

            return cita != null ? _mapper.Map<CitasDTos>(cita) : null;
        }

        public async Task<CitasDTos?> CrearCitaAsync(CitasDTos dto)
        {
            var clienteExiste = await _context.Usuarios.AnyAsync(u => u.UsuariosId == dto.ClienteId);
            var barberoExiste = await _context.Barberos.AnyAsync(b => b.BarberosId == dto.BarberosId);
            var servicioExiste = await _context.Servicios.AnyAsync(s => s.ServiciosId == dto.ServiciosId);
            if (!clienteExiste || !barberoExiste || !servicioExiste)
                return null;

            var horarioOcupado = await _context.Citas.AnyAsync(c =>
                c.BarberosId == dto.BarberosId && c.FechaHora == dto.FechaHora);
            if (horarioOcupado)
                return null;

            var cita = _mapper.Map<Citas>(dto);
            cita.Estado = "Pendiente";

            _context.Citas.Add(cita);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return null;
            }

            var guardada = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Barberos)!.ThenInclude(b => b.Usuario)
                .Include(c => c.Servicios)
                .FirstOrDefaultAsync(c => c.CitasId == cita.CitasId);

            return guardada != null ? _mapper.Map<CitasDTos>(guardada) : _mapper.Map<CitasDTos>(cita);
        }

        private static readonly string[] EstadosValidos =
            ["Pendiente", "Confirmada", "En curso", "Completada", "Cancelada"];

        public async Task<CitasDTos?> CambiarEstadoAsync(int id, string estado)
        {
            if (!EstadosValidos.Contains(estado))
                return null;

            var cita = await _context.Citas.FirstOrDefaultAsync(c => c.CitasId == id);
            if (cita is null)
                return null;

            cita.Estado = estado;
            await _context.SaveChangesAsync();
            return await GetCitaByIdAsync(id);
        }

        public async Task<bool> EliminarCitaAsync(int id)
        {
            var cita = await _context.Citas.FirstOrDefaultAsync(c => c.CitasId == id);
            if (cita is null)
                return false;

            _context.Citas.Remove(cita);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

