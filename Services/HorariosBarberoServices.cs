using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class HorariosBarberoServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public HorariosBarberoServices(StyleBookBarberBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 🔹 Obtener todos los horarios
        public async Task<IEnumerable<HorariosBarberoDTos>> GetHorariosAsync()
        {
            var horarios = await _context.HorariosBarbero.ToListAsync();
            return _mapper.Map<IEnumerable<HorariosBarberoDTos>>(horarios);
        }

        // 🔹 Obtener un horario por Id
        public async Task<HorariosBarberoDTos?> GetHorarioByIdAsync(int id)
        {
            var horario = await _context.HorariosBarbero.FindAsync(id);
            return horario != null ? _mapper.Map<HorariosBarberoDTos>(horario) : null;
        }

        // 🔹 Crear un nuevo horario
        public async Task<HorariosBarberoDTos> CrearHorarioAsync(HorariosBarberoDTos dto)
        {
            var horario = _mapper.Map<HorariosBarbero>(dto);
            _context.HorariosBarbero.Add(horario);
            await _context.SaveChangesAsync();
            return _mapper.Map<HorariosBarberoDTos>(horario);
        }

        // 🔹 Actualizar un horario existente
        public async Task<HorariosBarberoDTos?> ActualizarHorarioAsync(int id, HorariosBarberoDTos dto)
        {
            var horario = await _context.HorariosBarbero.FindAsync(id);
            if (horario == null) return null;

            _mapper.Map(dto, horario);
            await _context.SaveChangesAsync();
            return _mapper.Map<HorariosBarberoDTos>(horario);
        }

        // 🔹 Eliminar un horario
        public async Task<bool> EliminarHorarioAsync(int id)
        {
            var horario = await _context.HorariosBarbero.FindAsync(id);
            if (horario == null) return false;

            _context.HorariosBarbero.Remove(horario);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

