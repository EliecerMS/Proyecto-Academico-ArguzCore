using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.SupervisorProyectos
{
    [Authorize(Roles = "SupervisorProyectos")]
    public class ListaProyectosModel : PageModel
    {
        private readonly IProyectoService _proyectoService;

        public ListaProyectosModel(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
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
        }

        public async Task<IActionResult> OnPostCreateAsync() // crear un nuevo proyecto
        {
            await _proyectoService.CrearProyectoAsync(NuevoProyecto);

            return RedirectToPage(); // Recargar la página
        }

        public async Task<IActionResult> OnPostEditAsync() //edita el proyecto
        {
            await _proyectoService.ActualizarProyectoAsync(ProyectoEditado);
            return RedirectToPage(); // Recargar la página
        }

        public async Task<IActionResult> OnPostDeleteAsync(string CodigoProyecto) //elimina el proyecto
        {
            await _proyectoService.EliminarProyectoAsync(CodigoProyecto);
            return RedirectToPage(); // Recargar la página
        }
    }
}
