using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using CleanArchIdentityDemo.Infrastructure.Services;
using CleanArchIdentityDemo.WebUI;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using IdentityDbContext = CleanArchIdentityDemo.Infrastructure.Identity.ApplicationDbContext;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Configurar para leer variables de Azure App Service
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddEnvironmentVariables();
}

// Conexión a SQL Server con manejo de Azure
builder.Services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' no encontrado. " +
            "Verifica la configuración en Azure App Service.");
    }

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
        sqlOptions.CommandTimeout(180);
    });
});

// Registrar los servicios al iniciar el programa
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IEquipoService, EquipoService>();
builder.Services.AddScoped<IFinanzasService, FinanzasService>();
builder.Services.AddScoped<IMaterialesService, MaterialesService>();
builder.Services.AddScoped<IDocumentosService, DocumentosService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IContratoService, ContratoService>();
builder.Services.AddScoped<IAnaliticaService, AnaliticaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IBDRespladosService, BDRespaldoService>();

builder.Services.AddHttpContextAccessor();

// Configurar Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    }
})
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddRazorPages(options =>
{
    // options.Conventions.AuthorizeFolder("/");
});

// Configuración de límites de archivos
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024;
});

//Logging simplificado (sin AddAzureWebAppDiagnostics)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// En Azure App Service, los logs se envían automáticamente a Azure Monitor
// No es necesario AddAzureWebAppDiagnostics explícitamente

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

//Seed data con nombres de variables únicos
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var seedLogger = services.GetRequiredService<ILogger<Program>>(); // Nombre único

        seedLogger.LogInformation("Iniciando seed de datos...");

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Aplicar migraciones automáticamente en Azure
        if (app.Environment.IsProduction())
        {
            seedLogger.LogInformation("Aplicando migraciones pendientes...");
            context.Database.Migrate();
        }

        await SeedData.InitializeAsync(userManager, roleManager, context);

        seedLogger.LogInformation("Seed de datos completado exitosamente");
    }
    catch (Exception ex)
    {
        var errorLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); // Nombre único
        errorLogger.LogError(ex, "Error durante el seed de datos");

        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

//Logger al iniciar con nombre único
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>(); //Nombre único
startupLogger.LogInformation("Aplicación iniciada - Entorno: {Environment}", app.Environment.EnvironmentName);

app.Run();