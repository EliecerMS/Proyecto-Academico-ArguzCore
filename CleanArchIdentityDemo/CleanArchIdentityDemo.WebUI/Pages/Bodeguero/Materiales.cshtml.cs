using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Bodeguero
{
    [Authorize(Roles = "Bodeguero")]
    public class MaterialesModel : PageModel
    {
        private readonly IMaterialesService _MaterialesService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public MaterialesModel(IMaterialesService materialesService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _MaterialesService = materialesService;
            _UserService = userService;
            _UserManager = userManager;
        }

        public void OnGet()
        {
        }
    }
}
