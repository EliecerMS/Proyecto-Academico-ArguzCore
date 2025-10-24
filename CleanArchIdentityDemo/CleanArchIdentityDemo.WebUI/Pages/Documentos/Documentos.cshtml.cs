using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Documentos
{
    [Authorize(Roles = "Administrador,SupervisorProyectos,Contador")]
    public class DocumentosModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
