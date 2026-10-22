using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias del servicio de HorariosBarbero.
    /// Verifica el mapeo especial entre TimeSpan (entidad) y string "hh:mm" (DTO).
    /// </summary>
    public class HorariosBarberoServicesTests : IDisposable
    {
        private readonly StyleBookBarberBDContext _context;
        private readonly IMapper _mapper;
        private readonly HorariosBarberoServices _service;

        public HorariosBarberoServicesTests()
        {
            var options = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new StyleBookBarberBDContext(options);

            _mapper = Helpers.TestMapper.Create();

            _service = new HorariosBarberoServices(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact(DisplayName = "UT-HOR-01: GetHorariosAsync retorna los horarios con formato hh:mm")]
        public async Task GetHorariosAsync_ReturnsListWithFormattedTimes()
        {
            // Arrange
            _context.HorariosBarbero.Add(new HorariosBarbero
            {
                HorariosId = 1,
                BarberosId = 1,
                DiaSemana = 1,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(17, 0, 0)
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetHorariosAsync();

            // Assert
            result.Should().HaveCount(1);
            var horario = result.First();
            horario.HoraInicio.Should().Be("08:00");
            horario.HoraFin.Should().Be("17:00");
            horario.BarberoId.Should().Be(1);
        }

        [Fact(DisplayName = "UT-HOR-02: GetHorarioByIdAsync con Id existente retorna el horario")]
        public async Task GetHorarioByIdAsync_ExistingId_ReturnsHorario()
        {
            // Arrange
            _context.HorariosBarbero.Add(new HorariosBarbero
            {
                HorariosId = 5,
                BarberosId = 2,
                DiaSemana = 3,
                HoraInicio = new TimeSpan(9, 30, 0),
                HoraFin = new TimeSpan(18, 0, 0)
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetHorarioByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.HorariosId.Should().Be(5);
            result.HoraInicio.Should().Be("09:30");
        }

        [Fact(DisplayName = "UT-HOR-03: GetHorarioByIdAsync con Id inexistente retorna null")]
        public async Task GetHorarioByIdAsync_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _service.GetHorarioByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-HOR-04: CrearHorarioAsync convierte las horas string e inserta")]
        public async Task CrearHorarioAsync_ValidDto_ParsesTimesAndInserts()
        {
            // Arrange
            var dto = new HorariosBarberoDTos { BarberoId = 3, DiaSemana = 2, HoraInicio = "10:15", HoraFin = "19:45" };

            // Act
            var result = await _service.CrearHorarioAsync(dto);

            // Assert
            result.HorariosId.Should().BeGreaterThan(0);
            result.HoraInicio.Should().Be("10:15");
            var guardado = await _context.HorariosBarbero.FindAsync(result.HorariosId);
            guardado!.BarberosId.Should().Be(3);
            guardado.HoraFin.Should().Be(new TimeSpan(19, 45, 0));
        }

        [Fact(DisplayName = "UT-HOR-05: ActualizarHorarioAsync con Id existente actualiza los datos")]
        public async Task ActualizarHorarioAsync_ExistingId_UpdatesHorario()
        {
            // Arrange
            _context.HorariosBarbero.Add(new HorariosBarbero
            {
                HorariosId = 8,
                BarberosId = 1,
                DiaSemana = 1,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(12, 0, 0)
            });
            await _context.SaveChangesAsync();

            var dto = new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 5, HoraInicio = "14:00", HoraFin = "20:00" };

            // Act
            var result = await _service.ActualizarHorarioAsync(8, dto);

            // Assert
            result.Should().NotBeNull();
            result!.DiaSemana.Should().Be(5);
            result.HoraInicio.Should().Be("14:00");
            (await _context.HorariosBarbero.FindAsync(8))!.HoraFin.Should().Be(new TimeSpan(20, 0, 0));
        }

        [Fact(DisplayName = "UT-HOR-06: ActualizarHorarioAsync con Id inexistente retorna null")]
        public async Task ActualizarHorarioAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var dto = new HorariosBarberoDTos { BarberoId = 1, DiaSemana = 1, HoraInicio = "08:00", HoraFin = "09:00" };

            // Act
            var result = await _service.ActualizarHorarioAsync(4040, dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "UT-HOR-07: EliminarHorarioAsync con Id existente elimina y retorna true")]
        public async Task EliminarHorarioAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _context.HorariosBarbero.Add(new HorariosBarbero { HorariosId = 3, BarberosId = 1, DiaSemana = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EliminarHorarioAsync(3);

            // Assert
            result.Should().BeTrue();
            (await _context.HorariosBarbero.FindAsync(3)).Should().BeNull();
        }

        [Fact(DisplayName = "UT-HOR-08: EliminarHorarioAsync con Id inexistente retorna false")]
        public async Task EliminarHorarioAsync_NonExistentId_ReturnsFalse()
        {
            // Act
            var result = await _service.EliminarHorarioAsync(6060);

            // Assert
            result.Should().BeFalse();
        }
    }
}
