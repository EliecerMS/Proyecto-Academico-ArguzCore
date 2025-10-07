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

        public List<UserDto> UsuariosEmpleado { get; set; } = new List<UserDto>();
        public List<TareaDto> Tareas { get; private set; } // Este es el elemento donde se guardan las tareas

        [BindProperty]
        public TareaDto NuevaTarea { get; set; } = new TareaDto(); // Propiedad para enlazar el formulario de nueva tarea y poder crearla
                                                                   // Solicitud Material
        public class NuevoMaterialInput
        {
            public int MaterialId { get; set; }
            public int Cantidad { get; set; }
        }
        public List<SolicitudMaterial> SolicitudesMaterial { get; set; } = new();
        public List<Material> MaterialesDisponibles { get; set; } = new();

        [BindProperty]
        public SolicitudMaterial NuevaSolicitud { get; set; } = new SolicitudMaterial
        {
            MaterialesSolicitados = new List<MaterialSolicitado>
            {
            new MaterialSolicitado()
            }
        };


        [BindProperty]
        public List<NuevoMaterialInput> NuevosMateriales { get; set; } = new()
        {
            new NuevoMaterialInput()
        };




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

            // Traer proyecto completo usando el CódigoProyecto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto) ?? new Proyecto();

            // Traer las tareas relacionadas usando el IdProyecto con el metodo que ya tienes en el servicio
            if (DetalleProyecto != null)
            {
                Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();
            }
            //Cargar solicitudes de material del proyecto
            SolicitudesMaterial = (await _proyectoService.MostrarSolicitudesPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();

            //Cargar materiales disponibles para mostrar en el combo
            MaterialesDisponibles = (await _proyectoService.ObtenerMaterialesAsync()).ToList();

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

            // Redirigir a la misma página con el CódigoProyecto
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });

        }


        public async Task<IActionResult> OnPostEliminarTareaAsync(int IdTarea, string CodigoProyecto)
        {
            // Eliminar la tarea
            await _proyectoService.EliminarTareaAsync(IdTarea);

            // Recargar el proyecto completo para ovalidar que no se haya roto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

            /*if (DetalleProyecto == null)
            {
                return NotFound("Tarea no encontrada");
            }*/ //validar con mensaje de error, return NotFound lo que hace es mostrar una pagina de error 404, no es lo ideal, es mas correcto mostrar un mensaje en la misma pagina

            // Recargar lista de tareas
            //Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();

            // Redirigir a la misma página con el CódigoProyecto
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEditarTareaAsync()
        {
            // Actualizar la tarea
            await _proyectoService.EditarTareaAsync(NuevaTarea);
            // Recargar el proyecto completo para ovalidar que no se haya roto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                return NotFound("Tarea no encontrada");
            }
            // Recargar lista de tareas
            Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();
            // Redirigir a la misma página con el CódigoProyecto
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }
        //Solicitud de Materiales  

        public async Task<IActionResult> OnPostCrearSolicitudAsync()
        {
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                return NotFound("Proyecto no encontrado");
            }

            NuevaSolicitud.ProyectoId = DetalleProyecto.IdProyecto;
            NuevaSolicitud.FechaSolicitud = DateTime.Now;
            NuevaSolicitud.EstadoSolicitud = "Abierta";

            foreach (var mat in NuevaSolicitud.MaterialesSolicitados)
            {
                mat.SolicitudMaterial = NuevaSolicitud;

            }

            await _proyectoService.CrearSolicitudMaterialAsync(NuevaSolicitud);

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }



        public async Task<IActionResult> OnPostEditarSolicitudAsync(int IdSolicitud, int MaterialId, int Cantidad, string Prioridad, string Observaciones, string CodigoProyecto)
        {
            var solicitud = await _proyectoService.ObtenerSolicitudPorIdAsync(IdSolicitud);
            if (solicitud == null || solicitud.EstadoSolicitud != "Abierta")
            {
                return NotFound("Solicitud no encontrada o ya cerrada");
            }


            var materialSolicitado = solicitud.MaterialesSolicitados.FirstOrDefault();
            if (materialSolicitado != null)
            {
                materialSolicitado.MaterialId = MaterialId;
                materialSolicitado.Cantidad = Cantidad;
                materialSolicitado.Prioridad = Prioridad;
            }

            solicitud.ObservacionesBodeguero = Observaciones;

            await _proyectoService.ActualizarSolicitudAsync(solicitud);

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }
        public async Task<IActionResult> OnPostEliminarSolicitudAsync(int idSolicitud, string CodigoProyecto)
        {
            var solicitud = await _proyectoService.ObtenerSolicitudPorIdAsync(idSolicitud);
            if (solicitud == null)
            {
                TempData["ErrorPersonal"] = "La solicitud no fue encontrada o ya fue eliminada.";
                return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
            }

            await _proyectoService.EliminarSolicitudMaterialAsync(idSolicitud);

            TempData["MensajeExito"] = "La solicitud de material fue eliminada correctamente.";

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }
    }
}


