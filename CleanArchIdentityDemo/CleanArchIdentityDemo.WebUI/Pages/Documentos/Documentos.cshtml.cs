using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public DocumentosModel(IDocumentosService documentosService, IUserService userService, UserManager<ApplicationUser> userManager, IConfiguration configuration, IProyectoService proyectoService, IBlobStorageService blobStorageService)
        {
            _DocumentosService = documentosService;
            _UserService = userService;
            _UserManager = userManager;
            _configuration = configuration;
            _proyectoService = proyectoService;
            _blobStorageService = blobStorageService;
        }


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

        public async Task OnGet()
        {
            Documentos = (await _DocumentosService.ObtenerTodosLosDocumentosAsync()).ToList();
            Proyectos = (await _proyectoService.MostrarProyectosGeneralAsync()).ToList();

            // Obtener información del usuario autenticado desde Claims
            // ClaimTypes.NameIdentifier es el claim estándar para el ID de usuario en ASP.NET Identity
            UsuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // O también puede estar en este claim:
            // UsuarioId = User.FindFirst("sub")?.Value;

            RolUsuario = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
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
    }
}
