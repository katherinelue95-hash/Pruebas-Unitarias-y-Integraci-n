using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Fábrica de la aplicación para pruebas de integración.
    /// Reemplaza el DbContext real (SQL Server) por una base de datos en memoria
    /// y carga datos de prueba para poder ejercitar los endpoints reales.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Nombre único por instancia: aísla la base de datos entre clases de prueba.
        private readonly string _dbName = Guid.NewGuid().ToString();
        // Raíz compartida para que todos los DbContext de esta fábrica apunten a la misma base.
        private static readonly InMemoryDatabaseRoot DbRoot = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Quitar el registro de SQL Server que trae la app.
                // En EF Core 9/10 además de DbContextOptions<T> se registra
                // IDbContextOptionsConfiguration<T>, que hay que eliminar también.
                services.RemoveAll<DbContextOptions<StyleBookBarberBDContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<StyleBookBarberBDContext>>();
                services.RemoveAll<StyleBookBarberBDContext>();

                // Registrar la base de datos en memoria.
                services.AddDbContext<StyleBookBarberBDContext>(options =>
                    options.UseInMemoryDatabase(_dbName, DbRoot));

                // Crear la base y sembrar datos de prueba.
                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<StyleBookBarberBDContext>();
                db.Database.EnsureCreated();
                Seed(db);
            });
        }

        private static void Seed(StyleBookBarberBDContext db)
        {
            // Roles
            db.Roles.AddRange(
                new Roles { RolId = 1, NombreRol = "Cliente" },
                new Roles { RolId = 2, NombreRol = "Barbero" },
                new Roles { RolId = 3, NombreRol = "Administrador" });

            // Categorías
            db.Categorias.AddRange(
                new Categorias { CategoriasId = 1, NombreCategoria = "Cortes", Descripcion = "Cortes de cabello" },
                new Categorias { CategoriasId = 2, NombreCategoria = "Barba", Descripcion = "Servicios de barba" });

            // Usuarios (cliente, barbero, admin). El login compara correo + PasswordHash en texto.
            db.Usuarios.AddRange(
                new Usuarios { UsuariosId = 1, RolId = 1, Nombre = "Carlos", Apellido = "López", Correo = "cliente@test.com", PasswordHash = "clave123" },
                new Usuarios { UsuariosId = 2, RolId = 2, Nombre = "Andrés", Apellido = "Mora", Correo = "barbero@test.com", PasswordHash = "clave123" },
                new Usuarios { UsuariosId = 3, RolId = 3, Nombre = "Admin", Apellido = "Sistema", Correo = "admin@test.com", PasswordHash = "clave123" });

            // Barbero asociado al usuario 2
            db.Barberos.Add(new Barberos
            {
                BarberosId = 1,
                UsuariosId = 2,
                Especialidad = "Fade",
                FotoUrl = "foto.jpg",
                Calificacion = 4.5m,
                EstadoDisp = "Disponible"
            });

            // Servicios
            db.Servicios.AddRange(
                new Servicios { ServiciosId = 1, CategoriaId = 1, NombreServicio = "Corte clásico", Precio = 8000m, DuracionMin = 30, FotoUrl = null },
                new Servicios { ServiciosId = 2, CategoriaId = 2, NombreServicio = "Arreglo de barba", Precio = 5000m, DuracionMin = 20, FotoUrl = null });

            // Horario del barbero
            db.HorariosBarbero.Add(new HorariosBarbero
            {
                HorariosId = 1,
                BarberosId = 1,
                DiaSemana = 1,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(17, 0, 0)
            });

            // Cita existente (para pruebas de lectura y de colisión de horario)
            db.Citas.Add(new Citas
            {
                CitasId = 1,
                ClienteId = 1,
                BarberosId = 1,
                ServiciosId = 1,
                FechaHora = new DateTime(2026, 5, 1, 10, 0, 0),
                MontoTotal = 8000m,
                Estado = "Pendiente"
            });

            // Reseña existente
            db.Resenas.Add(new Resenas
            {
                ResenaId = 1,
                BarberosId = 1,
                UsuariosId = 1,
                Estrellas = 5,
                Comentario = "Excelente servicio",
                Fecha = new DateTime(2026, 4, 1, 12, 0, 0)
            });

            db.SaveChanges();
        }
    }
}
