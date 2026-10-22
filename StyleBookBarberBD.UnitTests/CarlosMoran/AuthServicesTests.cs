using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.DTOs;
using StyleBookBarberBD.Models;
using StyleBookBarberBD.Services;
using Xunit;

namespace StyleBookBarberBD.UnitTests;

public class AuthServicesTests : IDisposable
{
    private readonly StyleBookBarberBDContext db;
    private readonly AuthServices servicio;

    public AuthServicesTests()
    {
        var opciones = new DbContextOptionsBuilder<StyleBookBarberBDContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new StyleBookBarberBDContext(opciones);
        servicio = new AuthServices(db);

        db.Usuarios.Add(new Usuarios
        {
            UsuariosId = 1,
            Nombre = "Cliente",
            Apellido = "Prueba",
            Correo = "cliente@example.com",
            PasswordHash = "ClavePrueba123",
            RolId = 1
        });

        db.SaveChanges();
    }

    [Fact]
    public async Task Login_CredencialesValidas_RetornaUsuarioCorrecto()
    {
        var resultado = await servicio.LoginAsync(new AuthDTos
        {
            Correo = "cliente@example.com",
            PasswordHash = "ClavePrueba123"
        });

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.UsuariosId);
        Assert.Equal("cliente@example.com", resultado.Correo);
        Assert.Equal(1, resultado.RolId);
    }

    [Theory]
    [InlineData("cliente@example.com", "Incorrecta")]
    [InlineData("inexistente@example.com", "ClavePrueba123")]
    [InlineData("", "ClavePrueba123")]
    [InlineData("cliente@example.com", "")]
    public async Task Login_CredencialesInvalidas_RetornaNull(
        string correo, string clave)
    {
        var resultado = await servicio.LoginAsync(new AuthDTos
        {
            Correo = correo,
            PasswordHash = clave
        });

        Assert.Null(resultado);
        Assert.Equal(1, await db.Usuarios.CountAsync());
        Assert.Equal("ClavePrueba123",
            (await db.Usuarios.SingleAsync()).PasswordHash);
    }

    public void Dispose()
    {
        db.Database.EnsureDeleted();
        db.Dispose();
    }
}
