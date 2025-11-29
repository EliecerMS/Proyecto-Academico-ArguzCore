using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace CleanArchIdentityDemo.Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Ajustar ruta según donde esté el proyecto WebUI
            var webUIPath = @"C:\Users\eliec\OneDrive\Documents\GitHub\CleanArquitectureDemo\CleanArchIdentityDemo\CleanArchIdentityDemo.WebUI";


            // Construir configuración apuntando al proyecto WebUI
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(webUIPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true) // Habilitar User Secrets
                    .Build();

            // Obtener cadena de conexión
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Opciones del DbContext
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
