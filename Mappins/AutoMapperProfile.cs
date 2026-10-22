using AutoMapper;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // 🔹 Servicios
            CreateMap<Servicios, ServiciosDTos>()
                .ForMember(dest => dest.NombreCategoria, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.NombreCategoria : string.Empty))
                .ReverseMap();

            // 🔹 Citas
            CreateMap<Citas, CitasDTos>()
                .ForMember(dest => dest.BarberoNombre, opt => opt.MapFrom(src => src.Barberos != null && src.Barberos.Usuario != null ? $"{src.Barberos.Usuario.Nombre} {src.Barberos.Usuario.Apellido}".Trim() : string.Empty))
                .ForMember(dest => dest.ServicioNombre, opt => opt.MapFrom(src => src.Servicios != null ? src.Servicios.NombreServicio : string.Empty))
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : string.Empty));
            CreateMap<CitasDTos, Citas>()
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.Barberos, opt => opt.Ignore())
                .ForMember(dest => dest.Servicios, opt => opt.Ignore());

            // 🔹 Usuarios
            CreateMap<Usuarios, RegistroDTO>()
                .ForMember(dest => dest.NombreRol, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.NombreRol : string.Empty))
                .ReverseMap();

            // 🔹 Barberos
            CreateMap<Barberos, BarberosDTos>()
                .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.UsuariosId))
                .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.Usuario != null ? $"{src.Usuario.Nombre} {src.Usuario.Apellido}".Trim() : string.Empty))
                .ForMember(dest => dest.Correo, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Correo : string.Empty));
            CreateMap<BarberosDTos, Barberos>()
                .ForMember(dest => dest.UsuariosId, opt => opt.MapFrom(src => src.UsuarioId));
            CreateMap<Categorias, CategoriasDTos>()
                .ReverseMap();
            // 🔹 Reseñas
            CreateMap<Resenas, ResenasDTos>()
                .ForMember(dest => dest.Autor, opt => opt.MapFrom(src => src.Usuario != null ? $"{src.Usuario.Nombre} {src.Usuario.Apellido}".Trim() : string.Empty))
                .ForMember(dest => dest.BarberoNombre, opt => opt.MapFrom(src => src.Barberos != null && src.Barberos.Usuario != null ? $"{src.Barberos.Usuario.Nombre} {src.Barberos.Usuario.Apellido}".Trim() : string.Empty))
                .ReverseMap();

            // Roles
            CreateMap<Roles, RolesDTos>()
                .ForMember(dest => dest.RolesId, opt => opt.MapFrom(src => src.RolId));
            CreateMap<RolesDTos, Roles>()
                .ForMember(dest => dest.RolId, opt => opt.MapFrom(src => src.RolesId));
        }
    }
}


