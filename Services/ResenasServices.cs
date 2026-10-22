using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class ResenasServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public ResenasServices(StyleBookBarberBDContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ResenasDTos>> GetResenasAsync()
        {
            var resenas = await _context.Resenas
                .Include(r => r.Usuario)
                .Include(r => r.Barberos)!.ThenInclude(b => b.Usuario)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            var dtos = _mapper.Map<List<ResenasDTos>>(resenas);
            foreach (var dto in dtos)
                dto.CalificacionPromedio = await ObtenerPromedioAsync(dto.BarberosId);
            return dtos;
        }

        public async Task<List<ResenasDTos>> GetResenasByBarberoAsync(int barberoId)
        {
            var resenas = await _context.Resenas
                .Include(r => r.Usuario)
                .Include(r => r.Barberos)!.ThenInclude(b => b.Usuario)
                .Where(r => r.BarberosId == barberoId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            var dtos = _mapper.Map<List<ResenasDTos>>(resenas);
            var promedio = await ObtenerPromedioAsync(barberoId);
            foreach (var dto in dtos)
                dto.CalificacionPromedio = promedio;
            return dtos;
        }

        public async Task<ResenasDTos?> CrearResenaAsync(ResenasDTos dto)
        {
            // Validaciones: usuario real en la BD, barbero real y estrellas 1-5.
            if (dto.Estrellas < 1 || dto.Estrellas > 5)
                return null;

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuariosId == dto.UsuariosId);
            if (usuario is null)
                return null;

            var barbero = await _context.Barberos.FirstOrDefaultAsync(b => b.BarberosId == dto.BarberosId);
            if (barbero is null)
                return null;

            var resena = _mapper.Map<Resenas>(dto);
            resena.Fecha = DateTime.Now;

            _context.Resenas.Add(resena);

            // Recalcular la calificación promedio del barbero (incluyendo la nueva reseña)
            var conteo = await _context.Resenas.CountAsync(r => r.BarberosId == dto.BarberosId);
            var suma = await _context.Resenas.Where(r => r.BarberosId == dto.BarberosId).SumAsync(r => r.Estrellas);
            var promedio = (suma + dto.Estrellas) / (decimal)(conteo + 1);
            barbero.Calificacion = Math.Round(promedio, 2);

            await _context.SaveChangesAsync();

            var resultado = _mapper.Map<ResenasDTos>(resena);
            resultado.Autor = await ObtenerAutorAsync(usuario);
            resultado.CalificacionPromedio = barbero.Calificacion;
            return resultado;
        }

        private static async Task<string> ObtenerAutorAsync(Usuarios usuario)
            => await Task.FromResult($"{usuario.Nombre} {usuario.Apellido}".Trim());

        public async Task<bool> EliminarResenaAsync(int id)
        {
            var resena = await _context.Resenas.FirstOrDefaultAsync(r => r.ResenaId == id);
            if (resena is null)
                return false;

            var barberoId = resena.BarberosId;
            _context.Resenas.Remove(resena);
            await _context.SaveChangesAsync();

            // Recalcular el promedio del barbero sin la reseña eliminada.
            var barbero = await _context.Barberos.FirstOrDefaultAsync(b => b.BarberosId == barberoId);
            if (barbero is not null)
            {
                var conteo = await _context.Resenas.CountAsync(r => r.BarberosId == barberoId);
                var promedio = conteo == 0
                    ? 0m
                    : await _context.Resenas.Where(r => r.BarberosId == barberoId).AverageAsync(r => (decimal)r.Estrellas);
                barbero.Calificacion = Math.Round(promedio, 2);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        private async Task<decimal> ObtenerPromedioAsync(int barberoId)
        {
            var barbero = await _context.Barberos.FirstOrDefaultAsync(b => b.BarberosId == barberoId);
            return barbero?.Calificacion ?? 0m;
        }
    }
}