using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using StyleBookBarberBD.Mappings;

namespace StyleBookBarberBD.UnitTests.Helpers
{
    /// <summary>
    /// Crea una instancia de IMapper para las pruebas.
    /// Se usa la misma configuración que la API (AddAutoMapper sobre el ensamblado
    /// donde viven los perfiles), ya que AutoMapper 16 construye el mapper vía DI.
    /// </summary>
    public static class TestMapper
    {
        public static IMapper Create()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile).Assembly);
            return services.BuildServiceProvider().GetRequiredService<IMapper>();
        }
    }
}
