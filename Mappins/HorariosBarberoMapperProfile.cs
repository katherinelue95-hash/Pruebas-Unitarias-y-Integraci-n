using AutoMapper;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Mappings
{
    public class HorariosBarberoMapperProfile : Profile
    {
        public HorariosBarberoMapperProfile()
        {
            CreateMap<HorariosBarbero, HorariosBarberoDTos>()
                .ForMember(dest => dest.BarberoId, opt => opt.MapFrom(src => src.BarberosId))
                .ForMember(dest => dest.HoraInicio, opt => opt.MapFrom(src => src.HoraInicio.ToString(@"hh\:mm")))
                .ForMember(dest => dest.HoraFin, opt => opt.MapFrom(src => src.HoraFin.ToString(@"hh\:mm")));

            CreateMap<HorariosBarberoDTos, HorariosBarbero>()
                .ForMember(dest => dest.HorariosId, opt => opt.Ignore())
                .ForMember(dest => dest.BarberosId, opt => opt.MapFrom(src => src.BarberoId))
                .ForMember(dest => dest.HoraInicio, opt => opt.MapFrom(src => TimeSpan.Parse(src.HoraInicio)))
                .ForMember(dest => dest.HoraFin, opt => opt.MapFrom(src => TimeSpan.Parse(src.HoraFin)));
        }
    }
}

