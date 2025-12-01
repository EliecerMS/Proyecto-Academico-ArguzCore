using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.SupervisorProyectos
{
    [Authorize(Roles = "SupervisorProyectos,Administrador")]
    public class DashboardModel : PageModel
    {
        private readonly IProyectoService _proyectoService;
        private readonly IDashboardService _dashboardService;

        public DashboardModel(IProyectoService proyectoService, IDashboardService dashboardService)
        {
            _proyectoService = proyectoService;
            _dashboardService = dashboardService;
        }
        public IEnumerable<ProyectoDashboardDto> Proyectos { get; set; } = new List<ProyectoDashboardDto>();

        public async Task OnGetAsync()
        {
            Proyectos = await _proyectoService.MostrarProyectosActivosEInactivosAsync();
        }
        public async Task<IActionResult> OnGetDescargarInformeAsync(int proyectoId)
        {
            var pdfBytes = await _dashboardService.GenerarInformeProyectoAsync(proyectoId);
            return File(pdfBytes, "application/pdf", $"Informe_Proyecto_{proyectoId}.pdf");
        }
    }
}
