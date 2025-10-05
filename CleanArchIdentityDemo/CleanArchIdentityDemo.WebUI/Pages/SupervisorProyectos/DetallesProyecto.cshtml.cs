using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;


namespace CleanArchIdentityDemo.WebUI.Pages.SupervisorProyectos
{
    [Authorize(Roles = "SupervisorProyectos")]
    public class DetallesProyectoModel : PageModel
    {
        private readonly IProyectoService _proyectoService;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;


        public DetallesProyectoModel(IProyectoService proyectoService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _proyectoService = proyectoService;
            _userService = userService;
            _userManager = userManager;
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

        // ---------- VARIABLES PARA TAREAS
        public List<TareaDto> Tareas { get; private set; } // Este es el elemento donde se guardan las tareas

        [BindProperty]
        public TareaDto NuevaTarea { get; set; } = new TareaDto(); // Propiedad para enlazar el formulario de nueva tarea y poder crearla

        //Incidente
        public List<Incidente> Incidentes { get; set; } = new();

        // ---------- VARIABLES PARA NOTAS DE AVANCE
        [BindProperty]
        public NotaAvanceDto NuevaNota { get; set; }
        public List<NotaAvanceDto> Notas { get; set; } = new();
        [BindProperty]
        public Incidente NuevoIncidente { get; set; } = new Incidente();

        [BindProperty]
        public NotaAvanceDto EditarNota { get; set; } = new();



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


            // Traer las tareas relacionadas usando el IdProyecto con el metodo que ya tienes en el servicio
            if (DetalleProyecto != null)
            {
                Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();

                // Traer las notas relacionadas usando el IdProyecto
                Notas = (await _proyectoService.MostrarNotasAsync(DetalleProyecto.IdProyecto)).ToList();
                //Cargar Incidentes
                Incidentes = (await _proyectoService.MostrarIncidentesPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList(); 
            }
            else
            {
                // Inicializar listas vacías si no se encontró el proyecto
                Tareas = new List<TareaDto>();
                Notas = new List<NotaAvanceDto>();
            }



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

        // --------- METODOS PARA TAREAS
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
            TempData["TabActiva"] = "Cronograma";
            return RedirectToPage(new { CodigoProyecto });

        }

        public async Task<IActionResult> OnPostEliminarTareaAsync(int IdTarea, string CodigoProyecto)
        {
            // Eliminar la tarea
            await _proyectoService.EliminarTareaAsync(IdTarea);

            // Recargar el proyecto completo para ovalidar que no se haya roto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

            // Redirigir a la misma página con el CódigoProyecto
            TempData["TabActiva"] = "Cronograma";
            return RedirectToPage(new { CodigoProyecto });
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
            TempData["TabActiva"] = "Cronograma";
            return RedirectToPage(new { CodigoProyecto });
        }

        // --------- METODOS PARA NOTAS DE AVANCE
        public async Task<IActionResult> OnPostCrearNota()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Traer proyecto y validar que exista
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                TempData["Error"] = "No se encontró el proyecto. La nota no pudo crearse.";

                TempData["TabActiva"] = "NotasAvance";
                return RedirectToPage(new { CodigoProyecto }); // o redirige a un listado general
            }

            NuevaNota.CreadoPor = userId;
            NuevaNota.ProyectoId = DetalleProyecto.IdProyecto;
            NuevaNota.FechaNota = DateTime.Now;

            await _proyectoService.CrearNotaAsync(NuevaNota);

            TempData["MensajeExito"] = "Nota creada correctamente."; // Depuración, se puede borrar

            TempData["TabActiva"] = "NotasAvance";
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEditarNotaAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                TempData["Error"] = "No se encontró el proyecto. La nota no pudo actualizarse.";
                return RedirectToPage(new { CodigoProyecto });
            }

            NuevaNota.ProyectoId = DetalleProyecto.IdProyecto;
            NuevaNota.FechaNota = DateTime.Now;
            NuevaNota.CreadoPor = userId;

            await _proyectoService.EditarNotaAsync(NuevaNota);

            //TempData["MensajeExito"] = "Mensaje de depuracion";
            
            TempData["TabActiva"] = "NotasAvance";
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEliminarNotaAsync(int idNota)
        {
            await _proyectoService.EliminarNotaAsync(idNota);
            //TempData["MensajeExito"] = "Nota eliminada correctamente.";

            TempData["TabActiva"] = "NotasAvance";
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostDestacarNotaAsync(int idNota)
        {
            bool resultado = await _proyectoService.DestacarNotaAsync(idNota);

            /*if (!resultado)
            {
                TempData["Error"] = "No se encontró la nota seleccionada.";
            }
            else
            {
                TempData["MensajeExito"] = resultado
                    ? "La nota ha sido destacada correctamente."
                    : "La nota ya no está destacada.";
            }
            */
            TempData["TabActiva"] = "NotasAvance";
            return RedirectToPage(new { CodigoProyecto });
        }

        //Agregar Incidente
        public async Task<IActionResult> OnPostCrearIncidenteAsync()
        {
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                return NotFound("Proyecto no encontrado");
            }

            NuevoIncidente.ProyectoId = DetalleProyecto.IdProyecto;
            NuevoIncidente.FechaRegistro = DateTime.Now;
            NuevoIncidente.Estado = "Abierto";

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                NuevoIncidente.CreadoPor = user.Id;
            }

            await _proyectoService.CrearIncidenteAsync(NuevoIncidente);

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }
        public async Task<IActionResult> OnPostCerrarIncidenteAsync(int IdIncidente, string CodigoProyecto)
        {
            var incidente = await _proyectoService.ObtenerIncidentePorIdAsync(IdIncidente);
            if (incidente == null)
            {
                return NotFound("Incidente no encontrado");
            }

            incidente.Estado = "Cerrado";
            await _proyectoService.ActualizarIncidenteAsync(incidente);

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }
        public async Task<IActionResult> OnPostEditarIncidenteAsync(int IdIncidente, string Descripcion, string CodigoProyecto)
        {
            var incidente = await _proyectoService.ObtenerIncidentePorIdAsync(IdIncidente);
            if (incidente == null || incidente.Estado != "Abierto")
            {
                return NotFound("Incidente no encontrado o ya cerrado");
            }

            // Actualizar solo la descripción
            incidente.Descripcion = Descripcion;

            await _proyectoService.ActualizarIncidenteAsync(incidente);

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        } 
    }
}


