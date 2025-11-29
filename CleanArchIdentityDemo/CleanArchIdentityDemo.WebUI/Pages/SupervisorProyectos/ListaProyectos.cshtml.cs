using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.SupervisorProyectos
{
    [Authorize(Roles = "Administrador,SupervisorProyectos")]
    public class ListaProyectosModel : PageModel
    {
        private readonly IProyectoService _proyectoService;
        private readonly IAuditoriaService _auditoriaService;

        public ListaProyectosModel(IProyectoService proyectoService, IAuditoriaService auditoriaService)
        {
            _proyectoService = proyectoService;
            _auditoriaService = auditoriaService;
        }



        public List<ProyectoDto> Proyectos { get; set; } = new(); // almacenara la lista de proyectos

        // [BindProperty] es un atributo de Razor Pages que indica que una propiedad del PageModel debe enlazarse automáticamente a los datos que vienen de la solicitud HTTP, ya sea vía formulario (POST) o query string (GET).permite recibir datos del formulario en tu página sin tener que leerlos manualmente del Request.Form.

        [BindProperty]
        public ProyectoDto NuevoProyecto { get; set; } = new();

        [BindProperty]
        public ProyectoDto ProyectoEditado { get; set; } = new();




        public async Task OnGetAsync()//muestra los proyectos 
        {
            Proyectos = (await _proyectoService.MostrarProyectosAsync()).ToList();

            //Registra el acceso de los usuarios y lo guarda en la tabla de auditoria
            await _auditoriaService.RegistrarAccesoAsync("Dashboard");
        }

        public async Task<IActionResult> OnPostCreateAsync() // crear un nuevo proyecto
        {
            try
            {

                await _proyectoService.CrearProyectoAsync(NuevoProyecto);
                TempData["SuccessMessage"] = "Se creó el proyecto correctamente";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, registrar el error)
                TempData["ErrorMessage"] = "Error al crear el proyecto, intente de nuevo";
            }

            return RedirectToPage(); // Recargar la página
        }

        public async Task<IActionResult> OnPostEditAsync() //edita el proyecto
        {
            try
            {
                await _proyectoService.ActualizarProyectoAsync(ProyectoEditado);
                TempData["SuccessMessage"] = "Se editó el proyecto correctamente";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, registrar el error)
                TempData["ErrorMessage"] = "Error al editar el proyecto, intente de nuevo";
            }
            return RedirectToPage(); // Recargar la página
        }

        public async Task<IActionResult> OnPostDeleteAsync(string CodigoProyecto) //elimina el proyecto
        {
            try
            {
                await _proyectoService.EliminarProyectoAsync(CodigoProyecto);
                TempData["SuccessMessage"] = "Se eliminó el proyecto correctamente";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, registrar el error)
                TempData["ErrorMessage"] = "Error al eliminar el proyecto, intente de nuevo";
            }
            return RedirectToPage(); // Recargar la página
        }
    }
}
