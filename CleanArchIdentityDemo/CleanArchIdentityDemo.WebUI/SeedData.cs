using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchIdentityDemo.WebUI
{
    public class SeedData
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Administrador", "SupervisorProyectos", "Usuario" };
            string[] passwords = { "Admin123$", "Supervisor$1234", "Usuario$1234" };
            var i = 0;

            var usuarios = new List<ApplicationUser>();


            usuarios.Add(new ApplicationUser
            {
                UserName = "admin@demo.com",
                Email = "admin@demo.com",
                NombreCompleto = "Admin Name",

            });

            usuarios.Add(new ApplicationUser
            {
                UserName = "supervisor@demo.com",
                Email = "supervisor@demo.com",
                NombreCompleto = "Supervidor Prueba"
            });

            usuarios.Add(new ApplicationUser
            {
                UserName = "user@demo.com",
                Email = "user@demo.com",
                NombreCompleto = "Usuario Prueba"
            });


            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }


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
