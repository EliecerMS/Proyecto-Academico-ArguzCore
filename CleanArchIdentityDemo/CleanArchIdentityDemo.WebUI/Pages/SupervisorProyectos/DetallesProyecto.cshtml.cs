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
        private readonly IUserService _userService;

        public DetallesProyectoModel(IProyectoService proyectoService, IUserService userService)
        {
            _proyectoService = proyectoService;
            _userService = userService;
        }

        public Proyecto DetalleProyecto { get; set; }


        [BindProperty(SupportsGet = true)]
        public string CodigoProyecto { get; set; }

        [BindProperty]
        public int NuevoEstado { get; set; }

        [TempData]
        public string MensajeExito { get; set; }

        [BindProperty]
        public string UsuarioSeleccionado { get; set; } // Para asignar nuevo usuario

        [BindProperty]
        public string UsuarioReasignar { get; set; }

        [BindProperty]
        public string CodigoProyectoNuevo { get; set; }


        public List<ProyectoDto> ProyectosDisponibles { get; set; } = new();

        //lista de usuarios para asignar a un proyecto
        public List<PersonalAsignadoDto> PersonalAsignado { get; set; } = new();
        public List<UserDto> UsuariosDisponibles { get; set; } = new();


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

            //codigo aca abajo de otras cosas que se quieran cargar inmediatamente cargue esta vista

            // Personal asignado actualmente
            PersonalAsignado = (await _proyectoService.ObtenerPersonalPorProyectoAsync(CodigoProyecto)).ToList();

            // Lista de usuarios posibles
            UsuariosDisponibles = (await _userService.GetAllNormalUsersAsync()).ToList();


            ProyectosDisponibles = (await _proyectoService.MostrarProyectosListaReasignacionAsync(CodigoProyecto)).ToList();

        }
        //UsuariosEmpleado = await _userService.GetAllNormalUsersAsync().ToList();

        public async Task<IActionResult> OnPostAsignarPersonalAsync()
        {
            try
            {
                await _proyectoService.AsignarPersonalAProyectoAsync(CodigoProyecto, UsuarioSeleccionado);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorPersonal"] = ex.Message;
            }
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEliminarPersonalAsync(string personalId)
        {
            await _proyectoService.EliminarPersonalDeProyectoAsync(CodigoProyecto, personalId);
            return RedirectToPage(new { CodigoProyecto });
        }


        public async Task<IActionResult> OnPostReasignarPersonalAsync()
        {
            try
            {
                await _proyectoService.ReasignarPersonalEnProyectoAsync(CodigoProyecto, UsuarioReasignar, CodigoProyectoNuevo);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorPersonal"] = ex.Message;
            }
            return RedirectToPage(new { CodigoProyecto });

        }
    }
    }


