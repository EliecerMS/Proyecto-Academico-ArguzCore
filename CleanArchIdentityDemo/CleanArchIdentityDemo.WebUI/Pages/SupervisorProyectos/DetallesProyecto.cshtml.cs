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

        // Servicios para manejo de documentos y Blob Storage
        private readonly IDocumentosService _documentosService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public DetallesProyectoModel(IProyectoService proyectoService, IUserService userService, UserManager<ApplicationUser> userManager, IDocumentosService documentoService, IBlobStorageService blobStorageService, IConfiguration configuration)
        {
            _proyectoService = proyectoService;
            _userService = userService;
            _userManager = userManager;
            _documentosService = documentoService;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
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
                                                                   // Solicitud Material
        public class NuevoMaterialInput
        {
            public int MaterialId { get; set; }
            public int Cantidad { get; set; }
        }
        public List<SolicitudMaterialDto> SolicitudesMaterial { get; set; } = new();
        public List<MaterialDto> MaterialesDisponibles { get; set; } = new();

        //Incidente
        public List<Incidente> Incidentes { get; set; } = new();
        [BindProperty]
        public SolicitudMaterialDto NuevaSolicitud { get; set; } = new SolicitudMaterialDto
        {
            MaterialesSolicitados = new List<MaterialSolicitadoDto>
            {
                new MaterialSolicitadoDto()
            }
        };

        [BindProperty]
        public List<NuevoMaterialInput> NuevosMateriales { get; set; } = new()
        {
            new NuevoMaterialInput()
        };

        [BindProperty]
        public MaterialDto MaterialDevolver { get; set; } = new();

        // ---------- VARIABLES PARA NOTAS DE AVANCE
        [BindProperty]
        public NotaAvanceDto NuevaNota { get; set; }
        public List<NotaAvanceDto> Notas { get; set; } = new();
        [BindProperty]
        public Incidente NuevoIncidente { get; set; } = new Incidente();

        [BindProperty]
        public NotaAvanceDto EditarNota { get; set; } = new();

        public List<MaterialProyectoDto> MaterialesProyecto { get; set; } = new();

        [BindProperty]
        public DisminuirMaterialDto MaterialDisminuir { get; set; } = new();

        [BindProperty]
        public DisminuirMaterialDto MaterialEliminar { get; set; } = new();

        // PROPIEDADES PARA MODAL 1: DOCUMENTO VERSIONADO
        [BindProperty]
        public string NombreDocumentoVersionado { get; set; } = string.Empty;

        [BindProperty]
        public string? CategoriaDocumentoVersionado { get; set; }

        [BindProperty]
        public string? DescripcionDocumentoVersionado { get; set; }

        [BindProperty]
        public IFormFile ArchivoVersionado { get; set; } = null!;

        [BindProperty]
        public string? ComentariosVersionInicial { get; set; }

        // PROPIEDADES PARA MODAL 2: NUEVA VERSIÓN
        [BindProperty]
        public int DocumentoIdParaVersion { get; set; }

        [BindProperty]
        public IFormFile ArchivoNuevaVersion { get; set; } = null!;

        [BindProperty]
        public string? ComentariosNuevaVersion { get; set; }


        // PROPIEDADES PARA MODAL 3: DOCUMENTO SIMPLE
        [BindProperty]
        public string NombreDocumentoSimple { get; set; } = string.Empty;

        [BindProperty]
        public string? CategoriaDocumentoSimple { get; set; }

        [BindProperty]
        public string? DescripcionDocumentoSimple { get; set; }

        [BindProperty]
        public IFormFile ArchivoSimple { get; set; } = null!;

        public List<DocumentoDto> Documentos { get; set; } = new();

        // para validar rol usuario usuario Id y mostrar o no opciones en la vista
        public string RolUsuario { get; set; }
        public string UsuarioId { get; set; }

        public string SubidoPor { get; set; }
        public async Task<IActionResult> OnPostCambiarEstadoAsync()
        {
            try
            {
                await _proyectoService.CambiarEstadoAsync(CodigoProyecto, NuevoEstado);

                // Recargar el proyecto para reflejar cambios
                DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

                TempData["SuccessMessage"] = "El estado del proyecto se actualizó correctamente ";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al cambiar el estado del proyecto. Intente de nuevo.";
            }


            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });


        }

        public async Task OnGet(string CodigoProyecto)
        {
            // Aquí programacion para cargar todo absolutamente relacionado a un proyecto usando el CódigoProyecto
            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto) ?? new Proyecto();

            //codigo aca abajo de otras cosas que se quieran cargar inmediatamente cargue esta vista

            Documentos = (await _documentosService.ObtenerDocumentosPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();
            // Personal asignado actualmente
            PersonalAsignado = (await _proyectoService.ObtenerPersonalPorProyectoAsync(CodigoProyecto)).ToList();

            // Lista de usuarios posibles
            UsuariosDisponibles = (await _userService.GetAllNormalUsersAsync()).ToList();

            // Obtener información del usuario autenticado desde Claims
            // ClaimTypes.NameIdentifier es el claim estándar para el ID de usuario en ASP.NET Identity
            UsuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // O también puede estar en este claim:
            // UsuarioId = User.FindFirst("sub")?.Value;

            RolUsuario = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            ProyectosDisponibles = (await _proyectoService.MostrarProyectosListaReasignacionAsync(CodigoProyecto)).ToList();

            MaterialesProyecto = (await _proyectoService.ObtenerMaterialProyectoAsync(DetalleProyecto.IdProyecto)).ToList();

            // Traer las tareas relacionadas usando el IdProyecto con el metodo que ya tienes en el servicio
            if (DetalleProyecto != null)
            {
                Tareas = (await _proyectoService.MostrarTareasPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();

                // Traer las notas relacionadas usando el IdProyecto
                Notas = (await _proyectoService.MostrarNotasAsync(DetalleProyecto.IdProyecto)).ToList();
                //Cargar Incidentes
                Incidentes = (await _proyectoService.MostrarIncidentesPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();
                //Cargar solicitudes de material del proyecto
                SolicitudesMaterial = (await _proyectoService.MostrarSolicitudesPorProyectoAsync(DetalleProyecto.IdProyecto)).ToList();
            }
            else
            {
                // Inicializar listas vacías si no se encontró el proyecto
                Tareas = new List<TareaDto>();
                Notas = new List<NotaAvanceDto>();
            }


            //Cargar materiales disponibles para mostrar en el combo
            MaterialesDisponibles = (await _proyectoService.ObtenerMaterialesAsync()).ToList();

            //TempData["TabActiva"] = "marcaEntradaSalida";

        }
        //UsuariosEmpleado = await _userService.GetAllNormalUsersAsync().ToList();

        public async Task<IActionResult> OnPostAsignarPersonalAsync()
        {
            try
            {
                await _proyectoService.AsignarPersonalAProyectoAsync(CodigoProyecto, UsuarioSeleccionado);
                TempData["SuccessMessage"] = "Personal asignado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorPersonal"] = ex.Message;
            }
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEliminarPersonalAsync(string personalId)
        {
            try
            {
                await _proyectoService.EliminarPersonalDeProyectoAsync(CodigoProyecto, personalId);
                TempData["SuccessMessage"] = "Personal eliminado del proyecto correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorPersonal"] = ex.Message;
            }
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostReasignarPersonalAsync()
        {
            try
            {
                await _proyectoService.ReasignarPersonalEnProyectoAsync(CodigoProyecto, UsuarioReasignar, CodigoProyectoNuevo);
                TempData["SuccessMessage"] = "Personal reasignado correctamente.";
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
            try
            {
                await _proyectoService.CrearTareaAsync(NuevaTarea);


                // Redirigir a la misma página con el CódigoProyecto

                TempData["SuccessMessage"] = "Tarea creada correctamente.";
                TempData["TabActiva"] = "Cronograma";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la tarea";
            }
            return RedirectToPage(new { CodigoProyecto });

        }

        public async Task<IActionResult> OnPostEliminarTareaAsync(int IdTarea, string CodigoProyecto)
        {
            try
            {
                // Eliminar la tarea
                await _proyectoService.EliminarTareaAsync(IdTarea);

                // Recargar el proyecto completo para ovalidar que no se haya roto
                DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

                // Redirigir a la misma página con el CódigoProyecto
                TempData["SuccessMessage"] = "Tarea eliminada correctamente.";
                TempData["TabActiva"] = "Cronograma";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la tarea";
            }

            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEditarTareaAsync()
        {
            try
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
                TempData["SuccessMessage"] = "Tarea editada correctamente.";
                TempData["TabActiva"] = "Cronograma";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al editar la tarea";
            }
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
            try
            {

                await _proyectoService.CrearNotaAsync(NuevaNota);

                TempData["SuccessMessage"] = "Nota creada correctamente."; // Depuración, se puede borrar

                TempData["TabActiva"] = "NotasAvance";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al crear la nota. Intente de nuevo.";
            }
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEditarNotaAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);
            if (DetalleProyecto == null)
            {
                TempData["ErrorMessage"] = "No se encontró el proyecto. La nota no pudo actualizarse.";
                return RedirectToPage(new { CodigoProyecto });
            }

            NuevaNota.ProyectoId = DetalleProyecto.IdProyecto;
            NuevaNota.FechaNota = DateTime.Now;
            NuevaNota.CreadoPor = userId;
            try
            {
                await _proyectoService.EditarNotaAsync(NuevaNota);

                TempData["SuccessMessage"] = "Nota editada correctamente";

                TempData["TabActiva"] = "NotasAvance";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al editar la nota. Intente de nuevo.";
            }
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEliminarNotaAsync(int idNota)
        {
            try
            {
                await _proyectoService.EliminarNotaAsync(idNota);
                //TempData["MensajeExito"] = "Nota eliminada correctamente.";
                TempData["SuccessMessage"] = "Nota eliminada correctamente.";
                TempData["TabActiva"] = "NotasAvance";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la nota. Intente de nuevo.";
            }
            return RedirectToPage(new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostDestacarNotaAsync(int idNota)
        {
            try
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
                TempData["SuccessMessage"] = "La nota ha sido marcada o desmarcada como importante correctamente.";
                TempData["TabActiva"] = "NotasAvance";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al destacar la nota. Intente de nuevo.";
            }
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
            try
            {
                await _proyectoService.CrearIncidenteAsync(NuevoIncidente);
                TempData["SuccessMessage"] = "Incidente creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al crear el incidente. Intente de nuevo.";
            }

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
            try
            {
                await _proyectoService.ActualizarIncidenteAsync(incidente);
                TempData["SuccessMessage"] = "Incidente cerrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al cerrar el incidente. Intente de nuevo.";
            }

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
            try
            {
                await _proyectoService.ActualizarIncidenteAsync(incidente);
                TempData["SuccessMessage"] = "Incidente editado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al editar el incidente. Intente de nuevo.";
            }

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
            try
            {
                NuevaSolicitud.ProyectoId = DetalleProyecto.IdProyecto;
                NuevaSolicitud.FechaSolicitud = DateTime.Now;
                NuevaSolicitud.EstadoSolicitud = "Abierta";

                await _proyectoService.CrearSolicitudMaterialAsync(NuevaSolicitud);
                TempData["SuccessMessage"] = "Solicitud de material creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al crear la solicitud de material. Intente de nuevo.";
            }

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEditarSolicitudAsync(int IdSolicitud, int MaterialId, int Cantidad, string Prioridad, string CodigoProyecto)
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
            try
            {
                await _proyectoService.ActualizarSolicitudAsync(solicitud);
                TempData["SuccessMessage"] = "Solicitud de material editada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al editar la solicitud de material. Intente de nuevo.";
            }

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEliminarSolicitudAsync(int idSolicitud, string CodigoProyecto)
        {
            var solicitud = await _proyectoService.ObtenerSolicitudPorIdAsync(idSolicitud);
            if (solicitud == null)
            {
                TempData["ErrorMessage"] = "La solicitud no fue encontrada o ya fue eliminada.";
                return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
            }
            try
            {
                await _proyectoService.EliminarSolicitudMaterialAsync(idSolicitud);

                TempData["SuccessMessage"] = "La solicitud de material fue eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la solicitud de material. Intente de nuevo.";
            }

            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostDisminuirMaterialObraAsync()
        {
            try
            {
                await _proyectoService.DisminuirMaterialObraAsync(MaterialDisminuir);
                TempData["SuccessMessage"] = "Cantidad de material disminuida correctamente.";

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al disminuir la cantidad de material. Intente de nuevo.";
            }
            TempData["TabActiva"] = "Materiales";
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostDevolverMaterialAsync()
        {
            try
            {
                // Primero, disminuir la cantidad en obra
                MaterialDisminuir.CantidadADisminuir = MaterialDevolver.CantidadDisponible;
                await _proyectoService.DisminuirMaterialObraAsync(MaterialDisminuir);
                // Luego, devolver el material a bodega central
                var resultado = await _proyectoService.DevolverMaterialAsync(MaterialDevolver);
                if (!resultado)
                {
                    TempData["ErrorMessage"] = "No se pudo devolver el material. Verifique e intente de nuevo.";

                }
                else
                {
                    TempData["SuccessMessage"] = "Material devuelto correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al devolver el material. Intente de nuevo.";
            }
            TempData["TabActiva"] = "Materiales";
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        public async Task<IActionResult> OnPostEliminarMaterialProyectoAsync()
        {
            try
            {
                // Primero, devolver todo el material a bodega central
                //MaterialDisminuir.CantidadADisminuir = MaterialDevolver.CantidadDisponible;
                await _proyectoService.DevolverMaterialAsync(MaterialDevolver);
                var resultado = await _proyectoService.EliminarMaterialObraAsync(MaterialEliminar.IdMaterialProyecto);
                if (!resultado)
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el material del proyecto. Verifique e intente de nuevo.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Material eliminado del proyecto correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el material del proyecto. Intente de nuevo.";
            }
            TempData["TabActiva"] = "Materiales";
            return RedirectToPage("/SupervisorProyectos/DetallesProyecto", new { CodigoProyecto });
        }

        // ==========================================
        //Metodos para documentos
        // ==========================================

        // HANDLER 1: CREAR DOCUMENTO VERSIONADO
        public async Task<IActionResult> OnPostCrearDocumentoVersionadoAsync()
        {
            // Validar campos requeridos
            if (ArchivoVersionado == null || string.IsNullOrWhiteSpace(NombreDocumentoVersionado))
            {
                TempData["ErrorMessage"] = "Complete todos los campos requeridos.";
                return RedirectToPage(new { CodigoProyecto });
            }

            try
            {
                // 1. Validar archivo
                if (!ValidarArchivo(ArchivoVersionado, out string mensajeError))
                {
                    TempData["ErrorMessage"] = mensajeError;
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Cargar proyecto para obtener IdProyecto
                DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

                if (DetalleProyecto == null)
                {
                    TempData["ErrorMessage"] = "Proyecto no encontrado.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 3. Subir archivo a Blob Storage
                string blobUrl;
                using (var stream = ArchivoVersionado.OpenReadStream())
                {
                    blobUrl = await _blobStorageService.SubirArchivoAsync(
                        stream,
                        ArchivoVersionado.FileName,
                        ArchivoVersionado.ContentType
                    );
                }

                // 4. Crear documento versionado en BD
                var dto = new CrearDocumentoVersionadoDto
                {
                    ProyectoId = DetalleProyecto.IdProyecto,
                    NombreDocumento = NombreDocumentoVersionado,
                    CategoriaDocumento = CategoriaDocumentoVersionado,
                    Descripcion = DescripcionDocumentoVersionado,
                    NombreArchivoOriginal = ArchivoVersionado.FileName,
                    RutaBlobCompleta = blobUrl,
                    TipoArchivo = ArchivoVersionado.ContentType,
                    TamanoBytes = ArchivoVersionado.Length,
                    CreadoPor = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    ComentariosVersion = ComentariosVersionInicial
                };

                await _documentosService.CrearDocumentoVersionadoAsync(dto);

                TempData["SuccessMessage"] = $"Documento versionado '{NombreDocumentoVersionado}' creado correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear documento versionado: {ex.Message}";
            }

            return RedirectToPage(new { CodigoProyecto });
        }

        // HANDLER 2: AGREGAR NUEVA VERSIÓN
        public async Task<IActionResult> OnPostAgregarVersionAsync()
        {
            // Validar campos requeridos
            if (ArchivoNuevaVersion == null || DocumentoIdParaVersion == 0)
            {
                TempData["ErrorMessage"] = "Seleccione un documento y un archivo.";
                return RedirectToPage(new { CodigoProyecto });
            }

            if (string.IsNullOrWhiteSpace(ComentariosNuevaVersion))
            {
                TempData["ErrorMessage"] = "Los comentarios de la versión son obligatorios.";
                return RedirectToPage(new { CodigoProyecto });
            }

            try
            {
                // 1. Validar archivo
                if (!ValidarArchivo(ArchivoNuevaVersion, out string mensajeError))
                {
                    TempData["ErrorMessage"] = mensajeError;
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Subir archivo a Blob Storage
                string blobUrl;
                using (var stream = ArchivoNuevaVersion.OpenReadStream())
                {
                    blobUrl = await _blobStorageService.SubirArchivoAsync(
                        stream,
                        ArchivoNuevaVersion.FileName,
                        ArchivoNuevaVersion.ContentType
                    );
                }

                // 3. Agregar versión en BD
                var dto = new AgregarVersionDto
                {
                    DocumentoId = DocumentoIdParaVersion,
                    NombreArchivoOriginal = ArchivoNuevaVersion.FileName,
                    RutaBlobCompleta = blobUrl,
                    TipoArchivo = ArchivoNuevaVersion.ContentType,
                    TamanoBytes = ArchivoNuevaVersion.Length,
                    SubidoPor = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    Comentarios = ComentariosNuevaVersion
                };

                await _documentosService.AgregarVersionAsync(dto);

                TempData["SuccessMessage"] = "Nueva versión agregada correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al agregar versión: {ex.Message}";
            }

            return RedirectToPage(new { CodigoProyecto });
        }

        // ==========================================
        // HANDLER 3: CREAR DOCUMENTO SIMPLE
        // ==========================================
        public async Task<IActionResult> OnPostCrearDocumentoSimpleAsync()
        {
            // Validar campos requeridos
            if (ArchivoSimple == null || string.IsNullOrWhiteSpace(NombreDocumentoSimple))
            {
                TempData["ErrorMessage"] = "Complete todos los campos requeridos.";
                return RedirectToPage(new { CodigoProyecto });
            }

            try
            {
                // 1. Validar archivo
                if (!ValidarArchivo(ArchivoSimple, out string mensajeError))
                {
                    TempData["ErrorMessage"] = mensajeError;
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Cargar proyecto
                DetalleProyecto = await _proyectoService.DetallesProyecto(CodigoProyecto);

                if (DetalleProyecto == null)
                {
                    TempData["ErrorMessage"] = "Proyecto no encontrado.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 3. Subir archivo a Blob Storage
                string blobUrl;
                using (var stream = ArchivoSimple.OpenReadStream())
                {
                    blobUrl = await _blobStorageService.SubirArchivoAsync(
                        stream,
                        ArchivoSimple.FileName,
                        ArchivoSimple.ContentType
                    );
                }

                // 4. Crear documento simple en BD
                var dto = new CrearDocumentoSimpleDto
                {
                    ProyectoId = DetalleProyecto.IdProyecto,
                    NombreDocumento = NombreDocumentoSimple,
                    CategoriaDocumento = CategoriaDocumentoSimple,
                    Descripcion = DescripcionDocumentoSimple,
                    NombreArchivoOriginal = ArchivoSimple.FileName,
                    RutaBlobCompleta = blobUrl,
                    TipoArchivo = ArchivoSimple.ContentType,
                    TamanoBytes = ArchivoSimple.Length,
                    CreadoPor = User.FindFirstValue(ClaimTypes.NameIdentifier)!
                };

                await _documentosService.CrearDocumentoSimpleAsync(dto);

                TempData["SuccessMessage"] = $"Documento '{NombreDocumentoSimple}' creado correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear documento: {ex.Message}";
            }

            return RedirectToPage(new { CodigoProyecto });
        }

        // HANDLER: DESCARGAR VERSIÓN ESPECÍFICA
        public async Task<IActionResult> OnGetDescargarVersionAsync(int idVersion)
        {
            try
            {
                // 1. Obtener información de la versión desde BD
                var version = await _documentosService.ObtenerVersionPorIdAsync(idVersion);

                if (version == null)
                {
                    TempData["ErrorMessage"] = "Versión no encontrada.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Descargar archivo desde Blob Storage
                var (stream, contentType) = await _blobStorageService.DescargarArchivoAsync(version.RutaBlobCompleta);

                // 3. Retornar archivo al navegador
                return File(stream, contentType, version.NombreArchivoOriginal);
            }
            catch (FileNotFoundException ex)
            {
                TempData["ErrorMessage"] = $"Archivo no encontrado: {ex.Message}";
                return RedirectToPage(new { CodigoProyecto });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar versión: {ex.Message}";
                return RedirectToPage(new { CodigoProyecto });
            }
        }

        // HANDLER: DESCARGAR DOCUMENTO SIMPLE
        public async Task<IActionResult> OnGetDescargarDocumentoAsync(int idDocumento)
        {
            try
            {
                // 1. Obtener información del documento desde BD
                var documento = await _documentosService.ObtenerDocumentoPorIdAsync(idDocumento);

                if (documento == null || documento.TipoDocumento != "Simple")
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Descargar archivo desde Blob Storage
                var (stream, contentType) = await _blobStorageService.DescargarArchivoAsync(documento.RutaBlobCompleta!);

                // 3. Retornar archivo al navegador
                return File(stream, contentType, documento.NombreArchivoOriginal!);
            }
            catch (FileNotFoundException ex)
            {
                TempData["ErrorMessage"] = $"Archivo no encontrado: {ex.Message}";
                return RedirectToPage(new { CodigoProyecto });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar documento: {ex.Message}";
                return RedirectToPage(new { CodigoProyecto });
            }
        }

        // HANDLER: MARCAR VERSIÓN COMO ACTUAL
        public async Task<IActionResult> OnPostMarcarComoActualAsync(int idVersion)
        {
            try
            {
                await _documentosService.MarcarVersionComoActualAsync(idVersion);
                TempData["SuccessMessage"] = "Versión marcada como actual correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al marcar versión como actual: {ex.Message}";
            }

            return RedirectToPage(new { CodigoProyecto });
        }

        // HANDLER: ELIMINAR DOCUMENTO (SOFT DELETE)
        public async Task<IActionResult> OnPostEliminarDocumentoAsync(int idDocumento)
        {
            try
            {
                // 1. Obtener documento para validar y obtener URL del blob (opcional)
                var documento = await _documentosService.ObtenerDocumentoPorIdAsync(idDocumento);

                if (documento == null)
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Eliminar documento de BD (soft delete)
                await _documentosService.EliminarDocumentoAsync(idDocumento);

                // 3. OPCIONAL: Eliminar físicamente de Blob Storage


                if (documento.TipoDocumento == "Simple" && !string.IsNullOrEmpty(documento.RutaBlobCompleta))
                {
                    await _blobStorageService.EliminarArchivoAsync(documento.RutaBlobCompleta);
                }
                else if (documento.TipoDocumento == "Versionado")
                {
                    foreach (var version in documento.Versiones)
                    {
                        await _blobStorageService.EliminarArchivoAsync(version.RutaBlobCompleta);
                    }
                }


                TempData["SuccessMessage"] = "Documento eliminado correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar documento: {ex.Message}";
            }

            return RedirectToPage(new { CodigoProyecto });
        }


        // HANDLER: ELIMINAR VERSIÓN ESPECÍFICA
        public async Task<IActionResult> OnPostEliminarVersionAsync(int idVersion)
        {
            try
            {
                // 1. Obtener versión para validar
                var version = await _documentosService.ObtenerVersionPorIdAsync(idVersion);

                if (version == null)
                {
                    TempData["ErrorMessage"] = "Versión no encontrada.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Eliminar versión de BD (soft delete)
                await _documentosService.EliminarVersionAsync(idVersion);

                // 3. OPCIONAL: Eliminar físicamente de Blob Storage
                await _blobStorageService.EliminarArchivoAsync(version.RutaBlobCompleta);

                TempData["SuccessMessage"] = "Versión eliminada correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar versión: {ex.Message}";
            }

            return RedirectToPage(new { CodigoProyecto });
        }

        // MÉTODO AUXILIAR: VALIDAR ARCHIVO
        private bool ValidarArchivo(IFormFile archivo, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar que el archivo no esté vacío
            if (archivo == null || archivo.Length == 0)
            {
                mensajeError = "Por favor seleccione un archivo válido.";
                return false;
            }

            // Validar tamaño máximo
            var maxSizeMB = _configuration.GetValue<int>("AzureBlobStorage:MaxFileSizeMB");
            var maxSizeBytes = maxSizeMB * 1024 * 1024;

            if (archivo.Length > maxSizeBytes)
            {
                mensajeError = $"El archivo es demasiado grande. Tamaño máximo: {maxSizeMB} MB. Tamaño del archivo: {(archivo.Length / 1024.0 / 1024.0):N2} MB.";
                return false;
            }

            // Validar extensión permitida
            var allowedExtensions = _configuration.GetSection("AzureBlobStorage:AllowedExtensions").Get<string[]>();

            if (allowedExtensions != null && allowedExtensions.Length > 0)
            {
                var fileExtension = Path.GetExtension(archivo.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    mensajeError = $"Tipo de archivo no permitido. Extensiones válidas: {string.Join(", ", allowedExtensions)}";
                    return false;
                }
            }

            return true;
        }

        //Ver documentos

        //Ver documento simple en pestaña nueva
        public async Task<IActionResult> OnGetVerDocumentoAsync(int idDocumento)
        {
            try
            {
                // 1. Obtener documento desde BD
                var documento = await _documentosService.ObtenerDocumentoPorIdAsync(idDocumento);

                if (documento == null || string.IsNullOrEmpty(documento.RutaBlobCompleta))
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                // 2. Generar URL temporal con SAS Token (válida por 60 minutos)
                var urlTemporal = await _blobStorageService.ObtenerUrlTemporalAsync(
                    documento.RutaBlobCompleta,
                    duracionMinutos: 60
                );

                // 3. Redirigir a la URL temporal
                return Redirect(urlTemporal);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al acceder al documento: {ex.Message}";
                return RedirectToPage(new { CodigoProyecto });
            }
        }

        //ver version en pestaña nueva
        public async Task<IActionResult> OnGetVerVersionAsync(int idVersion)
        {
            try
            {
                var version = await _documentosService.ObtenerVersionPorIdAsync(idVersion);

                if (version == null || string.IsNullOrEmpty(version.RutaBlobCompleta))
                {
                    TempData["ErrorMessage"] = "Versión no encontrada.";
                    return RedirectToPage(new { CodigoProyecto });
                }

                var urlTemporal = await _blobStorageService.ObtenerUrlTemporalAsync(
                    version.RutaBlobCompleta,
                    duracionMinutos: 60
                );

                return Redirect(urlTemporal);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al acceder a la versión: {ex.Message}";
                return RedirectToPage(new { CodigoProyecto });
            }
        }
    }
}


