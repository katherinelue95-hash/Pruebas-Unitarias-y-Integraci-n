using Microsoft.EntityFrameworkCore;
using StyleBookBarberBD.Data;
using StyleBookBarberBD.EndPoints;
using StyleBookBarberBD.Mappings;
using StyleBookBarberBD.Services;
using AutoMapper;


var builder = WebApplication.CreateBuilder(args);

// 🔹 Configuración de DbContext con SQL Server
builder.Services.AddDbContext<StyleBookBarberBDContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile).Assembly);


// 🔹 Registro de Services

builder.Services.AddScoped<ServiciosServices>();
builder.Services.AddScoped<CitasServices>();
builder.Services.AddScoped<UsuariosServices>();
builder.Services.AddScoped<BarberosServices>();
builder.Services.AddScoped<CategoriasServices>();
builder.Services.AddScoped<HorariosBarberoServices>();
builder.Services.AddScoped<RolesServices>();
builder.Services.AddScoped<AuthServices>();
builder.Services.AddScoped<ResenasServices>();

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔹 Swagger (habilitado en todos los entornos: RunASP.NET sirve la app en Production)
app.UseSwagger();
app.UseSwaggerUI();

// 🔹 Middleware
app.UseHttpsRedirection();

// 🔹 Raíz: evita HTTP 404 en https://stylebookbarber-api.runasp.net/ y apunta a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// 🔹 Diagnóstico: estado del servidor y conexión real a la base de datos remota
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));

app.MapGet("/health/db", async (StyleBookBarberBDContext db, ILogger<Program> logger) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync();
        if (connected)
            return Results.Ok(new { database = "connected" });

        return Results.Ok(new { database = "no-connection", reason = "CanConnectAsync devolvió false" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo conectar con la base de datos remota.");
        return Results.Ok(new { database = "error", detail = ex.Message, inner = ex.InnerException?.Message });
    }
});

// 🔹 Archivos estáticos (fotos subidas por el panel administrativo)
app.UseStaticFiles();

// 🔹 Endpoints (Minimal API)

app.MapServiciosEndpoints();
app.MapCitasEndpoints();
app.MapUsuariosEndpoints();
app.MapBarberosEndpoints();
app.MapCategoriasEndpoints();
app.MapHorariosBareroEndpoints();
app.MapRolesEndpoints();
app.MapAuthEndpoints();
app.MapResenasEndpoints();
app.MapArchivosEndpoints();

app.Run();

