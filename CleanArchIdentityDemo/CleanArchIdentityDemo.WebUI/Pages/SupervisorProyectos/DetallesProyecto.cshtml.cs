using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;

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
        public List<TareaDto> Tareas { get; private set; } // Este es el elemento donde se guardan las tareas
        
        [BindProperty]
        public TareaDto NuevaTarea { get; set; } = new TareaDto(); // Propiedad para enlazar el formulario de nueva tarea y poder crearla



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


            // Traer proyecto completo usando el CódigoProyecto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto) ?? new Proyecto();

            // Traer las tareas relacionadas usando el IdProyecto con el metodo que ya tienes en el servicio
            if (DetalleProyecto != null)
            {
                Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();
            }
        }

        public async Task<IActionResult> OnPostCrearTareaAsync()
        {
            // Recargar el proyecto completo antes de usarlo para acceder a IdProyecto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                // Manejo de error si no se encuentra el proyecto
                return NotFound("Proyecto no encontrado");
            }

            // Ahora sí podemos asignar IdProyecto
            NuevaTarea.ProyectoId = DetalleProyecto.IdProyecto;

            // Crear la tarea
            await _proyectoService.CrearTareaAsync(NuevaTarea);

            // Recargar lista de tareas
            Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();

            // Redirigir a la misma página con el CódigoProyecto
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }





    }
}
