using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Models;

namespace StyleBookBarberBD.Data
{
    public class StyleBookBarberBDContext : DbContext
    {
        public StyleBookBarberBDContext(DbContextOptions<StyleBookBarberBDContext> options) : base(options) { }

        // Tablas (DbSet) que reflejan tu base de datos
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Barberos> Barberos { get; set; }
        public DbSet<HorariosBarbero> HorariosBarbero { get; set; }
        public DbSet<Servicios> Servicios { get; set; }
        public DbSet<Citas> Citas { get; set; }
        public DbSet<Resenas> Resenas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Restricción de colisión de citas (BarberoId + FechaHora)
            modelBuilder.Entity<Citas>()
                .HasIndex(c => new { c.BarberosId, c.FechaHora })
                .IsUnique()
                .HasDatabaseName("UQ_Citas_Barbero_FechaHora");

            // Relaciones con DeleteBehavior.Restrict para evitar borrados en cascada
            modelBuilder.Entity<Citas>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Citas>()
                .HasOne(c => c.Barberos)
                .WithMany()
                .HasForeignKey(c => c.BarberosId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Citas>()
                .HasOne(c => c.Servicios)
                .WithMany()
                .HasForeignKey(c => c.ServiciosId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Resenas>()
                .HasOne(r => r.Barberos)
                .WithMany()
                .HasForeignKey(r => r.BarberosId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Resenas>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuariosId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

