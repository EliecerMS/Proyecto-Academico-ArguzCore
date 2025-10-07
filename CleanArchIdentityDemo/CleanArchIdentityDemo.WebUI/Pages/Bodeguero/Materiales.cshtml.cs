using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Bodeguero
{
    [Authorize(Roles = "Bodeguero")]
    public class MaterialesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
