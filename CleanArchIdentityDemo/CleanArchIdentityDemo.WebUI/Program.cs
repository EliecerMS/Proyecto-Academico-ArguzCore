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

// Conexión a SQL Server local
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Registar los servicios al iniciar el programa
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IEquipoService, EquipoService>();
builder.Services.AddScoped<IFinanzasService, FinanzasService>();
builder.Services.AddScoped<IMaterialesService, MaterialesService>();
builder.Services.AddScoped<IDocumentosService, DocumentosService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IContratoService, ContratoService>();

// Configurar Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
})
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();  // línea clave para usar las páginas UI predeterminadas de Identity

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    //options.Conventions.AuthorizeFolder("/");  // Protege todas las páginas
});


// 4. CONFIGURACIÓN DE LÍMITES DE ARCHIVOS

builder.Services.Configure<FormOptions>(options =>
{
    // Límite de 50 MB para archivos subidos
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configuración de Kestrel para archivos grandes
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

// Si se usa IIS (opcional - solo si despliegas a IIS)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Obligatorio antes de Authorization
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    await SeedData.InitializeAsync(userManager, roleManager, context);
}

app.Run();
