using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Usuario
{
    [Authorize]
    [Authorize(Roles = "Usuario")]
    public class UsuarioModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
