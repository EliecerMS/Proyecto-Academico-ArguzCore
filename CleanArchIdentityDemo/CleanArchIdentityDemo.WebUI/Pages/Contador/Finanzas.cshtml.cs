using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Contador
{
    [Authorize(Roles = "Contador")]
    public class FinanzasModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
