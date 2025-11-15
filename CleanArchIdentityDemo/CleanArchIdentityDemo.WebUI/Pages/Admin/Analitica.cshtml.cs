using CleanArchIdentityDemo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    public class AnaliticaModel : PageModel
    {
        private readonly IAnaliticaService _analiticaService;

        public AnaliticaModel(IAnaliticaService analiticaService)
        {
            _analiticaService = analiticaService;

        }
        public void OnGet()
        {
        }
    }
}
