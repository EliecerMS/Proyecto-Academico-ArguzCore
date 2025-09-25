using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.SupervisorProyectos
{
    [Authorize(Roles = "SupervisorProyectos")]
    public class DetallesProyectoModel : PageModel
    {
        private readonly IProyectoService _proyectoService;

        public DetallesProyectoModel(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }
        public Proyecto? DetalleProyecto { get; set; } // almacena el detalle de un solo proyecto

        public async Task OnGet(string CodigoProyecto)
        {
            // Aquí programacion para cargar todo absolutamente relacionado a un proyecto usando el CódigoProyecto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto) ?? new Proyecto();
        }
    }
}
