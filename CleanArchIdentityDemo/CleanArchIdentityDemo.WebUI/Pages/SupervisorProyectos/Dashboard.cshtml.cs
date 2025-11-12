using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.SupervisorProyectos
{
    [Authorize(Roles = "SupervisorProyectos,Administrador,Contador")]
    public class DashboardModel : PageModel
    {
        private readonly IProyectoService _proyectoService;

        public IEnumerable<ProyectoDashboardDto> Proyectos { get; set; } = new List<ProyectoDashboardDto>();

        public DashboardModel(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }
        public async Task OnGetAsync()
        {
            Proyectos = await _proyectoService.MostrarProyectosActivosEInactivosAsync();
        }
    }
}
