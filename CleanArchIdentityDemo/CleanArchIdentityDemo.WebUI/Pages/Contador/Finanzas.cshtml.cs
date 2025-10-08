using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Contador
{
    [Authorize(Roles = "Contador")]
    public class FinanzasModel : PageModel
    {
        private readonly IFinanzasService _FinanzasService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public FinanzasModel(IFinanzasService finanzasService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _FinanzasService = finanzasService;
            _UserService = userService;
            _UserManager = userManager;
        }


        public void OnGet()
        {
        }
    }
}
