using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTos;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests;

public class RolesServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly RolesServices servicio;

    public RolesServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        db = new StyleBookBarberBDContext(opciones);
        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);
        servicio = new RolesServices(db, configuracion.CreateMapper());
    }

    private void Guardar(int id, string nombre)
    {
        db.Roles.Add(new Roles { RolId = id, NombreRol = nombre });
        db.SaveChanges();
    }

    [Fact]
    public async Task Listar_SinRoles_RetornaVacio()
    {
        Assert.Empty(await servicio.GetRolesAsync());
    }

    [Fact]
    public async Task Listar_ConRoles_RetornaIdentificadoresCorrectos()
    {
        Guardar(10, "Administrador");
        Guardar(20, "Cliente");
        var lista = await servicio.GetRolesAsync();
        Assert.Equal(2, lista.Count);
        Assert.Contains(lista, x => x.RolesId == 10 && x.NombreRol == "Administrador");
        Assert.Contains(lista, x => x.RolesId == 20 && x.NombreRol == "Cliente");
    }

    [Fact]
    public async Task Consultar_Existente_RetornaRol()
    {
        Guardar(10, "Barbero");
        var rol = await servicio.GetRolByIdAsync(10);
        Assert.NotNull(rol);
        Assert.Equal(10, rol.RolesId);
        Assert.Equal("Barbero", rol.NombreRol);
    }

    [Fact]
    public async Task Consultar_Inexistente_RetornaNull()
    {
        Assert.Null(await servicio.GetRolByIdAsync(999));
    }

    [Fact]
    public async Task Crear_Valido_GuardaYRetornaId()
    {
        var rol = await servicio.CrearRolAsync(new RolesDTos { NombreRol = "Recepcionista" });
        Assert.True(rol.RolesId > 0);
        var guardado = await db.Roles.FindAsync(rol.RolesId);
        Assert.NotNull(guardado);
        Assert.Equal("Recepcionista", guardado.NombreRol);
    }

    [Fact]
    public async Task Crear_DosRoles_GuardaAmbos()
    {
        var primero = await servicio.CrearRolAsync(new RolesDTos { NombreRol = "Jefe" });
        var segundo = await servicio.CrearRolAsync(new RolesDTos { NombreRol = "Cliente" });
        Assert.NotEqual(primero.RolesId, segundo.RolesId);
        Assert.Equal(2, await db.Roles.CountAsync());
    }

    [Fact]
    public async Task Actualizar_Existente_ConservaId()
    {
        Guardar(10, "Barbero");
        var actualizado = await servicio.ActualizarRolAsync(10,
            new RolesDTos { RolesId = 999, NombreRol = "Barbero senior" });
        Assert.NotNull(actualizado);
        Assert.Equal(10, actualizado.RolesId);
        Assert.Equal("Barbero senior", actualizado.NombreRol);
        Assert.Equal("Barbero senior", (await db.Roles.FindAsync(10))!.NombreRol);
    }

    [Fact]
    public async Task Actualizar_Inexistente_RetornaNull()
    {
        Assert.Null(await servicio.ActualizarRolAsync(999,
            new RolesDTos { NombreRol = "Ninguno" }));
    }

    [Fact]
    public async Task Eliminar_Existente_BorraRol()
    {
        Guardar(10, "Temporal");
        Assert.True(await servicio.EliminarRolAsync(10));
        Assert.Null(await db.Roles.FindAsync(10));
    }

    [Fact]
    public async Task Eliminar_Inexistente_RetornaFalse()
    {
        Assert.False(await servicio.EliminarRolAsync(999));
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
