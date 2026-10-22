using AutoMapper;
using FluentAssertions;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using Xunit;

namespace StyleBookBarberBD.UnitTests.Mappings
{
    /// <summary>
    /// Pruebas unitarias de los perfiles de AutoMapper.
    /// Verifican los mapeos personalizados (IDs, nombres relacionados y horas).
    /// Nota: no se valida la configuración completa con AssertConfigurationIsValid
    /// porque existen miembros no mapeados por diseño (por ejemplo CalificacionPromedio,
    /// que el servicio calcula, o la navegación Barbero).
    /// </summary>
    public class AutoMapperProfileTests
    {
        private readonly IMapper _mapper;

        public AutoMapperProfileTests()
        {
            _mapper = Helpers.TestMapper.Create();
        }

        [Fact(DisplayName = "UT-MAP-01: Servicios se mapea a ServiciosDTos con el nombre de la categoría")]
        public void Servicios_To_Dto_MapsCategoryName()
        {
            // Arrange
            var servicio = new Servicios
            {
                ServiciosId = 1,
                CategoriaId = 2,
                NombreServicio = "Corte",
                Precio = 8000m,
                DuracionMin = 30,
                Categoria = new Categorias { CategoriasId = 2, NombreCategoria = "Cortes" }
            };

            // Act
            var dto = _mapper.Map<ServiciosDTos>(servicio);

            // Assert
            dto.NombreServicio.Should().Be("Corte");
            dto.NombreCategoria.Should().Be("Cortes");
        }

        [Fact(DisplayName = "UT-MAP-02: Roles mapea RolId a RolesId y viceversa")]
        public void Roles_IdMapping_WorksInBothDirections()
        {
            // Arrange
            var rol = new Roles { RolId = 5, NombreRol = "Barbero" };

            // Act
            var dto = _mapper.Map<RolesDTos>(rol);
            var vuelta = _mapper.Map<Roles>(dto);

            // Assert
            dto.RolesId.Should().Be(5);
            vuelta.RolId.Should().Be(5);
        }

        [Fact(DisplayName = "UT-MAP-03: Barberos se mapea con NombreCompleto y Correo del usuario")]
        public void Barberos_To_Dto_MapsUserNameAndEmail()
        {
            // Arrange
            var barbero = new Barberos
            {
                BarberosId = 1,
                UsuariosId = 7,
                Especialidad = "Fade",
                Usuario = new Usuarios { UsuariosId = 7, Nombre = "Ana", Apellido = "Gómez", Correo = "ana@test.com" }
            };

            // Act
            var dto = _mapper.Map<BarberosDTos>(barbero);

            // Assert
            dto.UsuarioId.Should().Be(7);
            dto.NombreCompleto.Should().Be("Ana Gómez");
            dto.Correo.Should().Be("ana@test.com");
        }

        [Fact(DisplayName = "UT-MAP-04: HorariosBarbero convierte TimeSpan a string hh:mm y viceversa")]
        public void Horarios_TimeSpanMapping_WorksInBothDirections()
        {
            // Arrange
            var horario = new HorariosBarbero { HorariosId = 1, BarberosId = 3, DiaSemana = 1, HoraInicio = new TimeSpan(8, 30, 0), HoraFin = new TimeSpan(17, 0, 0) };

            // Act
            var dto = _mapper.Map<HorariosBarberoDTos>(horario);
            var vuelta = _mapper.Map<HorariosBarbero>(dto);

            // Assert
            dto.HoraInicio.Should().Be("08:30");
            dto.HoraFin.Should().Be("17:00");
            vuelta.HoraInicio.Should().Be(new TimeSpan(8, 30, 0));
            vuelta.BarberosId.Should().Be(3);
        }

        [Fact(DisplayName = "UT-MAP-05: Citas se mapea con los nombres de cliente, barbero y servicio")]
        public void Citas_To_Dto_MapsRelatedNames()
        {
            // Arrange
            var cita = new Citas
            {
                CitasId = 1,
                ClienteId = 1,
                BarberosId = 1,
                ServiciosId = 1,
                FechaHora = new DateTime(2026, 5, 1, 10, 0, 0),
                Cliente = new Usuarios { Nombre = "Carlos", Apellido = "López" },
                Barberos = new Barberos { Usuario = new Usuarios { Nombre = "Andrés", Apellido = "Mora" } },
                Servicios = new Servicios { NombreServicio = "Corte" }
            };

            // Act
            var dto = _mapper.Map<CitasDTos>(cita);

            // Assert
            dto.ClienteNombre.Should().Be("Carlos López");
            dto.BarberoNombre.Should().Be("Andrés Mora");
            dto.ServicioNombre.Should().Be("Corte");
        }
    }
}
