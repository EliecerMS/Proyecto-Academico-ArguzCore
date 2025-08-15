using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using CleanArchIdentityDemo.Infrastructure.Services;
using CleanArchIdentityDemo.WebUI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityDbContext = CleanArchIdentityDemo.Infrastructure.Identity.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);

// Conexión a SQL Server local
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Registar los servicios al iniciar el programa
builder.Services.AddScoped<IUserService, UserService>();


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
    await SeedData.InitializeAsync(userManager, roleManager);
}

app.Run();
