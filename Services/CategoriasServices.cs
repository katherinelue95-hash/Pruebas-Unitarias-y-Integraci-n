using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Services
{
    public class CategoriasServices
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;

        public CategoriasServices(
            StyleBookBarberBDContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Obtener todas las categorías
        public async Task<List<CategoriasDTos>> GetCategoriasAsync()
        {
            var categorias = await _context.Categorias.ToListAsync();

            return _mapper.Map<List<CategoriasDTos>>(categorias);
        }

        // Obtener categoría por Id
        public async Task<CategoriasDTos?> GetCategoriaByIdAsync(int id)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriasId == id);

            return categoria != null
                ? _mapper.Map<CategoriasDTos>(categoria)
                : null;
        }

        // Crear categoría
        public async Task<CategoriasDTos> CrearCategoriaAsync(CategoriasDTos dto)
        {
            var categoria = _mapper.Map<Categorias>(dto);

            _context.Categorias.Add(categoria);

            await _context.SaveChangesAsync();

            return _mapper.Map<CategoriasDTos>(categoria);
        }

        // Actualizar categoría
        public async Task<CategoriasDTos?> ActualizarCategoriaAsync(int id, CategoriasDTos dto)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return null;
            }

            categoria.NombreCategoria = dto.NombreCategoria;
            categoria.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return _mapper.Map<CategoriasDTos>(categoria);
        }

        // Eliminar categoría
        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return false;
            }

            _context.Categorias.Remove(categoria);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}