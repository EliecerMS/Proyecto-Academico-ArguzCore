using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    public class AnaliticaModel : PageModel
    {
        private readonly IAnaliticaService _analiticaService;
        private readonly IAuditoriaService _auditoriaService;

        public AnaliticaModel(IAuditoriaService auditoria, IAnaliticaService analiticaService)
        {
            _analiticaService = analiticaService;
            _auditoriaService = auditoria;

        }
        public async Task OnGet()
        {

            //Registra el acceso de los usuarios y lo guarda en la tabla de auditoria
            await _auditoriaService.RegistrarAccesoAsync("Materiales");
        }
    }
}
