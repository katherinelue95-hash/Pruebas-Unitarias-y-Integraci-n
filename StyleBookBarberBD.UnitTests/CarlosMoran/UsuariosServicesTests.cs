using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests;

public class UsuariosServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly UsuariosServices servicio;

    public UsuariosServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new StyleBookBarberBDContext(opciones);

        var configuracion = new MapperConfiguration(
            cfg => cfg.AddProfile<AutoMapperProfile>(),
            NullLoggerFactory.Instance);

        servicio = new UsuariosServices(db, configuracion.CreateMapper());

        db.Roles.AddRange(
            new Roles { RolId = 1, NombreRol = "Cliente" },
            new Roles { RolId = 2, NombreRol = "Barbero" });

        db.Usuarios.Add(new Usuarios
        {
            UsuariosId = 1,
            Nombre = "Ana",
            Apellido = "Prueba",
            Correo = "ana@example.com",
            PasswordHash = "ClavePrueba",
            RolId = 1
        });

        db.SaveChanges();
    }

    [Fact]
    public async Task CrearUsuario_AsignaRolClienteAunqueSoliciteOtro()
    {
        var resultado = await servicio.CrearUsuarioAsync(new RegistroDTO
        {
            Nombre = "Luis",
            Apellido = "Prueba",
            Correo = "luis@example.com",
            PasswordHash = "OtraClave",
            RolId = 2
        });

        Assert.True(resultado.UsuariosId > 1);
        Assert.Equal(1, resultado.RolId);

        var guardado = await db.Usuarios.FindAsync(resultado.UsuariosId);
        Assert.NotNull(guardado);
        Assert.Equal("luis@example.com", guardado.Correo);
        Assert.Equal(1, guardado.RolId);
        Assert.Equal(2, await db.Usuarios.CountAsync());
    }

    [Fact]
    public async Task ConsultarUsuarios_IncluyeDatosYNombreDelRol()
    {
        var resultado = await servicio.GetUsuariosAsync();

        var usuario = Assert.Single(resultado);
        Assert.Equal(1, usuario.UsuariosId);
        Assert.Equal("Ana", usuario.Nombre);
        Assert.Equal("Cliente", usuario.NombreRol);
    }

    [Fact]
    public async Task ConsultarPorId_Existente_RetornaUsuarioCorrecto()
    {
        var resultado = await servicio.GetUsuarioByIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal("ana@example.com", resultado.Correo);
        Assert.Equal("Cliente", resultado.NombreRol);
    }

    [Fact]
    public async Task ConsultarPorId_Inexistente_RetornaNull()
    {
        Assert.Null(await servicio.GetUsuarioByIdAsync(999));
    }

    [Fact]
    public async Task CambiarRol_Valido_PersisteNuevoRol()
    {
        var resultado = await servicio.CambiarRolAsync(1, 2);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.RolId);
        Assert.Equal("Barbero", resultado.NombreRol);

        db.ChangeTracker.Clear();
        var guardado = await db.Usuarios.FindAsync(1);
        Assert.NotNull(guardado);
        Assert.Equal(2, guardado.RolId);
    }

    [Fact]
    public async Task CambiarRol_ReferenciasInvalidas_NoModificaUsuario()
    {
        Assert.Null(await servicio.CambiarRolAsync(999, 2));
        Assert.Null(await servicio.CambiarRolAsync(1, 999));

        db.ChangeTracker.Clear();
        var usuario = await db.Usuarios.FindAsync(1);
        Assert.NotNull(usuario);
        Assert.Equal(1, usuario.RolId);
        Assert.Equal(1, await db.Usuarios.CountAsync());
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
