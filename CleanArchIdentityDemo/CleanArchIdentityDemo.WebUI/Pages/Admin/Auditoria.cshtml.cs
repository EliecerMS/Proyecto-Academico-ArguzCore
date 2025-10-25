using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class AuditoriaModel : PageModel
    {
        private readonly IAuditoriaService _AuditoriaService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public AuditoriaModel(IAuditoriaService auditoriaService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _AuditoriaService = auditoriaService;
            _UserService = userService;
            _UserManager = userManager;
        }
        public void OnGet()
        {
        }
    }
}
