
using Microsoft.AspNetCore.Identity;

namespace CleanArchIdentityDemo.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Campos adicionales de ser requerido
        public string NombreCompleto { get; set; }
        public bool Activo { get; set; } = true;
    }
}
