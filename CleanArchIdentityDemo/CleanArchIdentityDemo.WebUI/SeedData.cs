using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.WebUI
{
    public class SeedData
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            // Seed de Estados de Proyecto
            await SeedEstadosProyecto(context);

            // Seed Roles
            string[] roles = { "Administrador", "SupervisorProyectos", "Contador", "Bodeguero", "JefeMaquinaria" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Usuarios
            await SeedUsuarios(userManager, roles);
        }

        private static async Task SeedEstadosProyecto(ApplicationDbContext context)
        {
            // Insertar estados de proyectos

            var estados = new List<EstadoProyecto>
                {
                    new EstadoProyecto
                    {

                        NombreEstado = "Planificado"
                    },
                    new EstadoProyecto
                    {

                        NombreEstado = "En ejecuión"
                    },
                    new EstadoProyecto
                    {

                        NombreEstado = "Finalizado"
                    },
                    new EstadoProyecto
                    {

                        NombreEstado = "Ejemplo"
                    }
                };

            foreach (var estado in estados)
            {
                // Verificar si el estado ya existe para evitar duplicados, sino agregar alguno no existente
                var estadoExistente = await context.EstadosProyecto
                        .FirstOrDefaultAsync(e => e.NombreEstado == estado.NombreEstado);

                if (estadoExistente == null)
                {
                    context.EstadosProyecto.Add(estado);
                    await context.SaveChangesAsync();
                }

            }

        }


        private static async Task SeedUsuarios(UserManager<ApplicationUser> userManager, string[] roles)
        {
            string[] passwords = { "Admin123$", "Supervisor$1234", "Contador$1234", "Bodeguero$1234", "JefeMaquinaria$1234" };
            var i = 0;

            var usuarios = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "admin@demo.com",
                    Email = "admin@demo.com",
                    NombreCompleto = "Admin Name",
                },
                new ApplicationUser
                {
                    UserName = "supervisor@demo.com",
                    Email = "supervisor@demo.com",
                    NombreCompleto = "Supervisor Prueba"
                },
                new ApplicationUser
                {
                    UserName = "contador@demo.com",
                    Email = "contador@demo.com",
                    NombreCompleto = "Contador Prueba"
                },
                new ApplicationUser
                {
                    UserName = "bodeguero@demo.com",
                    Email = "bodeguero@demo.com",
                    NombreCompleto = "Bodeguero Prueba"
                },
                new ApplicationUser
                {
                    UserName = "jefemaquinaria@demo.com",
                    Email = "jefemaquinaria@demo.com",
                    NombreCompleto = "Jefe Maquinaria Prueba"
                }
            };

            foreach (var user in usuarios)
            {
                var userFind = await userManager.FindByEmailAsync(user.Email);
                if (userFind == null)
                {
                    await userManager.CreateAsync(user, passwords[i]);
                    await userManager.AddToRoleAsync(user, roles[i]);
                }
                i += 1;
            }
        }
    }

}
