using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public Proyecto DetalleProyecto { get; set; }


        [BindProperty]
        public string CodigoProyecto { get; set; }

        [BindProperty]
        public int NuevoEstado { get; set; }

        [TempData]
        public string MensajeExito { get; set; }


        public async Task<IActionResult> OnPostCambiarEstadoAsync()
        {
            await _proyectoService.CambiarEstadoAsync(CodigoProyecto, NuevoEstado);

            // Recargar el proyecto para reflejar cambios
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

            MensajeExito = "El estado del proyecto se actualizó correctamente ";


            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });


        }

        public async Task OnGet(string CodigoProyecto)
        {
            // Aquí programacion para cargar todo absolutamente relacionado a un proyecto usando el CódigoProyecto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto) ?? new Proyecto();
        }
    }
}
