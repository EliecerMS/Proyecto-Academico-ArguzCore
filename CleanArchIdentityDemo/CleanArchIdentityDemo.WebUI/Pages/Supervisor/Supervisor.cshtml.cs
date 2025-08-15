using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Supervisor
{
    [Authorize(Roles = "SupervisorProyectos")]
    public class SupervisorModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
