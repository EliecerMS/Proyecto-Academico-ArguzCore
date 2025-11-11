using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CleanArchIdentityDemo.WebUI.Pages.Documentos
{
    [Authorize(Roles = "Administrador,SupervisorProyectos,Contador")]
    public class DocumentosModel : PageModel
    {
        private readonly IDocumentosService _DocumentosService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IProyectoService _proyectoService;
        private readonly IContratoService _ContratoService;
        private readonly ApplicationDbContext _context;


        public DocumentosModel(IDocumentosService documentosService, IUserService userService, UserManager<ApplicationUser> userManager, IConfiguration configuration, IProyectoService proyectoService, IBlobStorageService blobStorageService, IContratoService contratoService,
            ApplicationDbContext context)
        {
            _DocumentosService = documentosService;
            _UserService = userService;
            _UserManager = userManager;
            _configuration = configuration;
            _proyectoService = proyectoService;
            _blobStorageService = blobStorageService;
            _ContratoService = contratoService;
            _context = context;
        }

        public List<ContratoDto> Contratos { get; private set; } = new();
        public List<ProyectoDto> ProyectosDisponibles { get; private set; } = new();
        [BindProperty]
        public ContratoDto NuevoContrato { get; set; } = new();
        [BindProperty]
        public ContratoDto EditarContrato { get; set; } = new();

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

        public List<ProyectoDto> Proyectos { get; set; } = new();

        [BindProperty]
        public int ProyectoId { get; set; }

        // para validar rol usuario usuario Id y mostrar o no opciones en la vista
        public string RolUsuario { get; set; }
        public string UsuarioId { get; set; }

        public string SubidoPor { get; set; }


        // Propiedades para filtros de búsqueda
        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }              // texto de búsqueda

        [BindProperty(SupportsGet = true)]
        public string? Categoria { get; set; }      // categoría seleccionada

        public IEnumerable<DocumentoDto> DocumentosFiltrados { get; private set; } = Enumerable.Empty<DocumentoDto>();
        public int TotalCoincidencias { get; private set; }

        public static readonly string[] TodasLasCategorias = new[]
        {
            "Plano","Contrato","Especificación","Informe","Memoria de Cálculo",
            "Presupuesto","Estudio","Factura","Fotografía","Certificado","Acta",
            "Orden de Compra","Carta","Comprobante","Licencia","Otro","Equipos", "Transporte"
        };

        public SelectList CategoriasSelect { get; private set; } = default!;

        public async Task OnGet()
        {
            Contratos = (await _ContratoService.GetAllAsync()).ToList();

            ProyectosDisponibles = (await _proyectoService.MostrarProyectosAsync()).ToList();

            Documentos = (await _DocumentosService.ObtenerTodosLosDocumentosAsync()).ToList();
            Proyectos = (await _proyectoService.MostrarProyectosGeneralAsync()).ToList();

            UsuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            RolUsuario = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            // Para aplicar filtros ======
            DocumentosFiltrados = Filtrar(Documentos, Q, Categoria);
            TotalCoincidencias = DocumentosFiltrados.Count();

            PrepararSelects();
        }


        // ==========================================
        //Metodos para documentos
        // ==========================================
        //public bool PuedeEliminarOMarcarVersionOAgregarVersion(string usuarioId)
        //{
        //    bool condicion = false;
        //    if (usuarioId.Equals(SubidoPor))
        //    {
        //        condicion = true;
        //    }
        //    return condicion;
        //}

        // HANDLER 1: CREAR DOCUMENTO VERSIONADO
        public async Task<IActionResult> OnPostCrearDocumentoVersionadoAsync()
        {
            // Validar campos requeridos
            if (ArchivoVersionado == null || string.IsNullOrWhiteSpace(NombreDocumentoVersionado))
            {
                TempData["ErrorMessage"] = "Complete todos los campos requeridos.";
                return RedirectToPage();
            }

            try
            {
                // 1. Validar archivo
                if (!ValidarArchivo(ArchivoVersionado, out string mensajeError))
                {
                    TempData["ErrorMessage"] = mensajeError;
                    return RedirectToPage();
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
                    ProyectoId = ProyectoId,
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

                await _DocumentosService.CrearDocumentoVersionadoAsync(dto);

                TempData["SuccessMessage"] = $"Documento versionado '{NombreDocumentoVersionado}' creado correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear documento versionado: {ex.Message}";
            }

            return RedirectToPage();
        }

        // HANDLER 2: AGREGAR NUEVA VERSIÓN
        public async Task<IActionResult> OnPostAgregarVersionAsync()
        {
            // Validar campos requeridos
            if (ArchivoNuevaVersion == null || DocumentoIdParaVersion == 0)
            {
                TempData["ErrorMessage"] = "Seleccione un documento y un archivo.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(ComentariosNuevaVersion))
            {
                TempData["ErrorMessage"] = "Los comentarios de la versión son obligatorios.";
                return RedirectToPage();
            }

            try
            {
                // 1. Validar archivo
                if (!ValidarArchivo(ArchivoNuevaVersion, out string mensajeError))
                {
                    TempData["ErrorMessage"] = mensajeError;
                    return RedirectToPage();
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

                await _DocumentosService.AgregarVersionAsync(dto);

                TempData["SuccessMessage"] = "Nueva versión agregada correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al agregar versión: {ex.Message}";
            }

            return RedirectToPage();
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
                return RedirectToPage();
            }

            try
            {
                // 1. Validar archivo
                if (!ValidarArchivo(ArchivoSimple, out string mensajeError))
                {
                    TempData["ErrorMessage"] = mensajeError;
                    return RedirectToPage();
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
                    ProyectoId = ProyectoId,
                    NombreDocumento = NombreDocumentoSimple,
                    CategoriaDocumento = CategoriaDocumentoSimple,
                    Descripcion = DescripcionDocumentoSimple,
                    NombreArchivoOriginal = ArchivoSimple.FileName,
                    RutaBlobCompleta = blobUrl,
                    TipoArchivo = ArchivoSimple.ContentType,
                    TamanoBytes = ArchivoSimple.Length,
                    CreadoPor = User.FindFirstValue(ClaimTypes.NameIdentifier)!
                };

                await _DocumentosService.CrearDocumentoSimpleAsync(dto);

                TempData["SuccessMessage"] = $"Documento '{NombreDocumentoSimple}' creado correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear documento: {ex.Message}";
            }

            return RedirectToPage();
        }

        // HANDLER: DESCARGAR VERSIÓN ESPECÍFICA
        public async Task<IActionResult> OnGetDescargarVersionAsync(int idVersion)
        {
            try
            {
                // 1. Obtener información de la versión desde BD
                var version = await _DocumentosService.ObtenerVersionPorIdAsync(idVersion);

                if (version == null)
                {
                    TempData["ErrorMessage"] = "Versión no encontrada.";
                    return RedirectToPage();
                }

                // 2. Descargar archivo desde Blob Storage
                var (stream, contentType) = await _blobStorageService.DescargarArchivoAsync(version.RutaBlobCompleta);

                // 3. Retornar archivo al navegador
                return File(stream, contentType, version.NombreArchivoOriginal);
            }
            catch (FileNotFoundException ex)
            {
                TempData["ErrorMessage"] = $"Archivo no encontrado: {ex.Message}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar versión: {ex.Message}";
                return RedirectToPage();
            }
        }

        // HANDLER: DESCARGAR DOCUMENTO SIMPLE
        public async Task<IActionResult> OnGetDescargarDocumentoAsync(int idDocumento)
        {
            try
            {
                // 1. Obtener información del documento desde BD
                var documento = await _DocumentosService.ObtenerDocumentoPorIdAsync(idDocumento);

                if (documento == null || documento.TipoDocumento != "Simple")
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    return RedirectToPage();
                }

                // 2. Descargar archivo desde Blob Storage
                var (stream, contentType) = await _blobStorageService.DescargarArchivoAsync(documento.RutaBlobCompleta!);

                // 3. Retornar archivo al navegador
                return File(stream, contentType, documento.NombreArchivoOriginal!);
            }
            catch (FileNotFoundException ex)
            {
                TempData["ErrorMessage"] = $"Archivo no encontrado: {ex.Message}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar documento: {ex.Message}";
                return RedirectToPage();
            }
        }

        // HANDLER: MARCAR VERSIÓN COMO ACTUAL
        public async Task<IActionResult> OnPostMarcarComoActualAsync(int idVersion)
        {
            try
            {
                await _DocumentosService.MarcarVersionComoActualAsync(idVersion);
                TempData["SuccessMessage"] = "Versión marcada como actual correctamente.";
                TempData["TabActiva"] = "Documentos";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al marcar versión como actual: {ex.Message}";
            }

            return RedirectToPage();
        }

        // HANDLER: ELIMINAR DOCUMENTO (SOFT DELETE)
        public async Task<IActionResult> OnPostEliminarDocumentoAsync(int idDocumento)
        {
            try
            {
                // 1. Obtener documento para validar y obtener URL del blob (opcional)
                var documento = await _DocumentosService.ObtenerDocumentoPorIdAsync(idDocumento);

                if (documento == null)
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    return RedirectToPage();
                }

                // 2. Eliminar documento de BD (soft delete)
                await _DocumentosService.EliminarDocumentoAsync(idDocumento);

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

            return RedirectToPage();
        }


        // HANDLER: ELIMINAR VERSIÓN ESPECÍFICA
        public async Task<IActionResult> OnPostEliminarVersionAsync(int idVersion)
        {
            try
            {
                // 1. Obtener versión para validar
                var version = await _DocumentosService.ObtenerVersionPorIdAsync(idVersion);

                if (version == null)
                {
                    TempData["ErrorMessage"] = "Versión no encontrada.";
                    return RedirectToPage();
                }

                // 2. Eliminar versión de BD (soft delete)
                await _DocumentosService.EliminarVersionAsync(idVersion);

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

            return RedirectToPage();
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
                var documento = await _DocumentosService.ObtenerDocumentoPorIdAsync(idDocumento);

                if (documento == null || string.IsNullOrEmpty(documento.RutaBlobCompleta))
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    return RedirectToPage();
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
                return RedirectToPage();
            }
        }

        //ver version en pestaña nueva
        public async Task<IActionResult> OnGetVerVersionAsync(int idVersion)
        {
            try
            {
                var version = await _DocumentosService.ObtenerVersionPorIdAsync(idVersion);

                if (version == null || string.IsNullOrEmpty(version.RutaBlobCompleta))
                {
                    TempData["ErrorMessage"] = "Versión no encontrada.";
                    return RedirectToPage();
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
                return RedirectToPage();
            }
        }
        public async Task<IActionResult> OnPostRegistrarContratoAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevoContrato.Descripcion))
                ModelState.AddModelError(nameof(NuevoContrato.Descripcion), "El nombre del empleado es requerido.");

            if (NuevoContrato.ProyectoId <= 0)
                ModelState.AddModelError(nameof(NuevoContrato.ProyectoId), "Debe seleccionar un proyecto.");

            if (NuevoContrato.FechaFin < NuevoContrato.FechaInicio)
                ModelState.AddModelError(nameof(NuevoContrato.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");


            ModelState.Remove("ArchivoSimple");
            ModelState.Remove("ArchivoVersionado");
            ModelState.Remove("ArchivoNuevaVersion");
            ModelState.Remove("NombreDocumentoSimple");
            ModelState.Remove("NombreDocumentoVersionado");
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor complete correctamente los campos del formulario.";
                TempData["TabActiva"] = "ContratosLaborales";
                return RedirectToPage("/Documentos/Documentos");
            }

            try
            {
                await _ContratoService.CreateAsync(NuevoContrato);
                TempData["SuccessMessage"] = "Contrato laboral registrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al registrar el contrato: {ex.Message}";
            }

            TempData["TabActiva"] = "ContratosLaborales";
            return RedirectToPage("/Documentos/Documentos");
        }

        public async Task<IActionResult> OnPostEditarContratoAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(EditarContrato.Descripcion))
                ModelState.AddModelError(nameof(EditarContrato.Descripcion), "El nombre del empleado es requerido.");

            if (EditarContrato.ProyectoId <= 0)
                ModelState.AddModelError(nameof(EditarContrato.ProyectoId), "Debe seleccionar un proyecto.");

            if (EditarContrato.FechaFin < EditarContrato.FechaInicio)
                ModelState.AddModelError(nameof(EditarContrato.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");

            ModelState.Remove("ArchivoSimple");
            ModelState.Remove("ArchivoVersionado");
            ModelState.Remove("ArchivoNuevaVersion");
            ModelState.Remove("NombreDocumentoSimple");
            ModelState.Remove("NombreDocumentoVersionado");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revise los datos del formulario.";
                TempData["TabActiva"] = "ContratosLaborales";
                return RedirectToPage("/Documentos/Documentos");
            }

            try
            {
                var ok = await _ContratoService.UpdateAsync(EditarContrato);
                TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                    ok ? "Contrato actualizado correctamente." : "No se pudo actualizar el contrato.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al actualizar el contrato: {ex.Message}";
            }

            TempData["TabActiva"] = "ContratosLaborales";
            return RedirectToPage("/Documentos/Documentos");
        }

        public async Task<IActionResult> OnPostEliminarContratoAsync(int IdContrato)
        {
            try
            {
                var ok = await _ContratoService.DeleteAsync(IdContrato);
                TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                    ok ? "Contrato eliminado correctamente." : "No se pudo eliminar el contrato.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el contrato: {ex.Message}";
            }

            TempData["TabActiva"] = "ContratosLaborales";
            return RedirectToPage("/Documentos/Documentos");
        }

        private static IEnumerable<DocumentoDto> Filtrar(IEnumerable<DocumentoDto> docs, string? q, string? categoria)
        {
            var query = docs;

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(d =>
                    (d.CategoriaDocumento ?? "").Equals(categoria, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                query = query.Where(d =>
                       (!string.IsNullOrEmpty(d.NombreDocumento) && d.NombreDocumento.Contains(t, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(d.Descripcion) && d.Descripcion.Contains(t, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(d.TipoDocumento) && d.TipoDocumento.Contains(t, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(d.NombreArchivoOriginal) && d.NombreArchivoOriginal.Contains(t, StringComparison.OrdinalIgnoreCase))
                    || (d.Versiones != null && d.Versiones.Any(v =>
                           (!string.IsNullOrEmpty(v.NombreArchivoOriginal) && v.NombreArchivoOriginal.Contains(t, StringComparison.OrdinalIgnoreCase))
                        || (!string.IsNullOrEmpty(v.Comentarios) && v.Comentarios.Contains(t, StringComparison.OrdinalIgnoreCase))
                    )));
            }

            // Ordenamiento
            return query.OrderByDescending(d => d.FechaSubida);
        }

        private void PrepararSelects()
        {
            // El cuarto parámetro es el valor seleccionado actual (Categoria)
            CategoriasSelect = new SelectList(TodasLasCategorias, Categoria);
        }

        public async Task<PartialViewResult> OnGetBuscarAsync(string? q, string? categoria)
        {
            // Carga base (puedes copiar lo esencial de tu OnGet)
            Documentos = (await _DocumentosService.ObtenerTodosLosDocumentosAsync()).ToList();

            // Asigna filtros
            Q = q;
            Categoria = categoria;

            // Aplica filtros
            DocumentosFiltrados = Filtrar(Documentos, Q, Categoria);
            TotalCoincidencias = DocumentosFiltrados.Count();

            // Construye y devuelve la parcial
            var vdd = new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState)
            {
                Model = this
            };
            return new PartialViewResult
            {
                ViewName = "_ResultadosDocumentos",
                ViewData = vdd
            };
        }


    }

}

