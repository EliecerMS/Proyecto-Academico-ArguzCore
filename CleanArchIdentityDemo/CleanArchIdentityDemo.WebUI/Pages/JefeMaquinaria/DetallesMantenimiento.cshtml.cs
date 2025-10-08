using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.JefeMaquinaria
{
    [Authorize(Roles = "JefeMaquinaria")]
    public class DetallesMantenimientoModel : PageModel
    {
        private readonly IEquipoService _EquipoService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public DetallesMantenimientoModel(IEquipoService equipoService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _EquipoService = equipoService;
            _UserService = userService;
            _UserManager = userManager;
        }
        public void OnGet()
        {
        }
    }
}
