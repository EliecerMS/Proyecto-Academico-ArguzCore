using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages
{
    [Authorize] // para ver esta pagina es necesario autenticarse
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;


        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
