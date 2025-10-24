using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Documentos
{
    [Authorize(Roles = "Administrador,SupervisorProyectos,Contador")]
    public class DocumentosModel : PageModel
    {
        private readonly IDocumentosService _DocumentosService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public DocumentosModel(IDocumentosService documentosService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _DocumentosService = documentosService;
            _UserService = userService;
            _UserManager = userManager;
        }

        public void OnGet()
        {
        }
    }
}
