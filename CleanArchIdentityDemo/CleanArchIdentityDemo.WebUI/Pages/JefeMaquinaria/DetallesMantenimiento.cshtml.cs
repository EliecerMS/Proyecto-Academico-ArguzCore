using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.JefeMaquinaria
{
    [Authorize(Roles = "JefeMaquinaria")]
    public class DetallesMantenimientoModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
