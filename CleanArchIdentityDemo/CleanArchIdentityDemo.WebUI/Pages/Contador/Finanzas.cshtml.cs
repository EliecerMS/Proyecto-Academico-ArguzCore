using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CleanArchIdentityDemo.WebUI.Pages.Contador
{
    [Authorize(Roles = "Contador")]
    public class FinanzasModel : PageModel
    {
        private readonly IFinanzasService _FinanzasService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IProyectoService _ProyectoService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IDocumentosService _DocumentosService;

        public FinanzasModel(IFinanzasService finanzasService, IUserService userService, UserManager<ApplicationUser> userManager, IProyectoService proyectoService, IBlobStorageService blobStorageService, IConfiguration configuration, IDocumentosService documentoService)
        {
            _FinanzasService = finanzasService;
            _UserService = userService;
            _UserManager = userManager;
            _ProyectoService = proyectoService;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _DocumentosService = documentoService;
        }

        public List<PagoProveedorDto> PagosProveedores { get; set; } = new(); // almacenara la lista de pagos a proveedores
        public List<ProveedorDto> Proveedores { get; set; } = new(); // almacenara la lista de proveedores

        [BindProperty]
        public ProveedorDto Proveedor { get; set; } = new(); // almacenara la lista de proveedores

        [BindProperty]
        public ProveedorDto ProveedorEditado { get; set; } = new(); // trae los datos del proveedor seleccionado para editar

        [BindProperty]
        public int PagoId { get; set; } //almacenara el Id del pago seleccionado para ver el recibo

        [BindProperty]
        public int IdProveedor { get; set; }

        [BindProperty]
        public PagoProveedorDto PagoSeleccionado { get; set; } = new();

        public List<ProyectoDto> Proyectos { get; set; } = new();

        //Costos ejecutados 
        [BindProperty]
        public CostoEjecutadoDto CostoSeleccionado { get; set; } = new();

        [BindProperty]
        public int IdCosto { get; set; }

        public List<CostoEjecutadoDto> CostosEjecutados { get; set; } = new();

        // PROPIEDADES PARA SUBIR EL COMPROBANTE (DOCUMENTO SIMPLE)
        [BindProperty]
        public string NombreDocumentoSimple { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile ArchivoSimple { get; set; } = null!;

        [BindProperty]
        public IFormFile ArchivoSimplePagoProveedor { get; set; } = null!;

        public List<ProveedorDto> ProveedoresTodos { get; set; } = new(); // almacenara la lista de proveedores

        public async Task OnGetAsync()
        {
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
            Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
            ProveedoresTodos = (await _FinanzasService.ListarTodosProveedoresAsync()).ToList();
            //TempData["TabActiva"] = "GestionProveedor";
            Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
        }
        public async Task<IActionResult> OnPostVerReciboAsync()
        {
            // GenerarComprobantePago es un método del servicio que genera el comprobante de pago en formato PDF y lo devuelve como un arreglo de bytes
            var reciboBytes = _FinanzasService.GenerarComprobante(PagoId);

            // Retornar el archivo en PDF
            return File(reciboBytes, "application/pdf");
        }
        public async Task<IActionResult> OnGetFiltrarPagosAsync(int? idProveedor, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Filtrar pagos
            var pagos = await _FinanzasService.ListarPagosProveedoresAsync();

            if (idProveedor.HasValue && idProveedor > 0)
            {
                pagos = pagos.Where(p => p.ProveedorId == idProveedor.Value);
            }

            if (fechaInicio.HasValue)
            {
                pagos = pagos.Where(p => p.FechaPago >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                pagos = pagos.Where(p => p.FechaPago <= fechaFin.Value);
            }

            // Retornar solo la partial view
            return Partial("_TablaPagosPartial", pagos.ToList());
        }

        public async Task<IActionResult> OnPostRegistrarProveedorAsync()
        {
            ModelState.Remove("Contacto");
            ModelState.Remove("NombreProveedor");
            // Data Annotations valida automáticamente
            if (!ModelState.IsValid)  //Aquí se valida automáticamente
            {
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                TempData["TabActiva"] = "GestionProveedor";
                return Page();  // Retorna la página CON errores
            }

            if (await _FinanzasService.CrearProveedorAsync(Proveedor))
            {
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                // Retornar la página (NO redirige, solo recarga)
                TempData["SuccessMessage"] = "Proveedor creado correctamente";
                TempData["TabActiva"] = "GestionProveedor";
                return Page();
            }
            else
            {
                TempData["ErrorMessage"] = "Error al crear proveedor";
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                TempData["TabActiva"] = "GestionProveedor";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditarProveedorAsync()
        {
            ModelState.Remove("Contacto");
            ModelState.Remove("NombreProveedor");
            // Data Annotations valida automáticamente
            if (!ModelState.IsValid)  //Aquí se valida automáticamente
            {
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                TempData["TabActiva"] = "GestionProveedor";
                return Page();  // Retorna la página CON errores
            }
            if (await _FinanzasService.EditarProveedorAsync(ProveedorEditado))
            {
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                // Retornar la página (NO redirige, solo recarga)
                TempData["SuccessMessage"] = "Proveedor editado correctamente";
                TempData["TabActiva"] = "GestionProveedor";
                return Page();
            }
            else
            {
                TempData["InfoMessage"] = "No hubo datos que editar para el proveedor";
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                TempData["TabActiva"] = "GestionProveedor";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEliminarProveedorAsync()
        {
            if (await _FinanzasService.EliminarProveedorAsync(IdProveedor))
            {
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                // Retornar la página (NO redirige, solo recarga)
                TempData["SuccessMessage"] = "Proveedor eliminado correctamente";
                TempData["TabActiva"] = "GestionProveedor";
                return Page();
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar proveedor. Verifique que no tenga pagos asociados.";
                // Cargar datos actualizados
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                TempData["TabActiva"] = "GestionProveedor";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostRegistrarPagoAsync()
        {
            // Ignorar TODO lo que no sea de PagoSeleccionado
            var keysToKeep = new[]
            {
                "PagoSeleccionado.ProveedorId",
                "PagoSeleccionado.ProyectoId",
                "PagoSeleccionado.Monto",
                "PagoSeleccionado.FechaPago",
                "PagoSeleccionado.Descripcion"
            };

            //Borra cualquier otra entrada del ModelState que no pertenezca al formulario de pago
            foreach (var key in ModelState.Keys.ToList())
            {
                if (!keysToKeep.Contains(key))
                    ModelState.Remove(key);
            }

            // Ver si sigue siendo inválido 
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Datos inválidos para registrar el pago.";
                TempData["TabActiva"] = "PagosProveedor";
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                return Page();
            }

            var documentoCreado = await CrearDocumentoSimplePagoProveedor();

            // Si la creación del documento falló (incluye excepción capturada), detener flujo y mostrar Page()
            if (!documentoCreado)
            {
                // Recargar datos necesarios para la vista
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                TempData["TabActiva"] = "PagosProveedor";
                return Page();
            }

            // Registrar el pago
            PagoSeleccionado.NombreDocumentoSubido = NombreDocumentoSimple;
            var resultado = await _FinanzasService.RegistrarPagoProveedorAsync(PagoSeleccionado);

            if (resultado != null)
            {
                TempData["SuccessMessage"] = "Pago registrado correctamente.";

                //Limpia el formulario y el estado del modelo
                ModelState.Clear();
                PagoSeleccionado = new PagoProveedorDto();
            }
            else
            {
                TempData["ErrorMessage"] = "Error al registrar el pago.";
            }

            // Recargar tablas y pestaña activa
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
            Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
            TempData["TabActiva"] = "PagosProveedor";
            Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
            return Page();
        }
        //Métodos para costos ejecutados 
        public async Task<IActionResult> OnPostRegistrarCostoAsync()
        {
            // Validar campos relevantes
            var keysToKeep = new[]
            {
               "CostoSeleccionado.ProyectoId",
               "CostoSeleccionado.CategoriaGasto",
               "CostoSeleccionado.Monto",
               "CostoSeleccionado.Fecha",
               "CostoSeleccionado.Descripcion"
           };

            foreach (var key in ModelState.Keys.ToList())
            {
                if (!keysToKeep.Contains(key))
                    ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Datos inválidos para registrar el costo ejecutado.";
                TempData["TabActiva"] = "GastosEjecutados";
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                return Page();
            }

            var documentoCreado = await CrearDocumentoSimple();

            // Si la creación del documento falló (incluye excepción capturada), detener flujo y mostrar Page()
            if (!documentoCreado)
            {
                // Recargar datos necesarios para la vista
                Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                TempData["TabActiva"] = "GastosEjecutados";
                return Page();
            }

            var resultado = await _FinanzasService.CrearCostoEjecutadoAsync(CostoSeleccionado);

            if (resultado)
            {
                TempData["SuccessMessage"] = "Costo ejecutado registrado correctamente.";
                ModelState.Clear();
                CostoSeleccionado = new CostoEjecutadoDto();
            }
            else
            {
                TempData["ErrorMessage"] = "Error al registrar el costo ejecutado.";
            }

            CostosEjecutados = (await _FinanzasService.ListarCostosEjecutadosAsync()).ToList();
            Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
            TempData["TabActiva"] = "GastosEjecutados";
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
            Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();

            return Page();
        }

        public async Task<PartialViewResult> OnGetVerGastosPorProyectoAsync(int idProyecto)
        {
            try
            {
                var lista = await _FinanzasService.ListarCostosPorProyectoAsync(idProyecto);

                // Evitar null (si no hay nada, devolver lista vacía)
                lista ??= new List<CostoEjecutadoDto>();

                return new PartialViewResult
                {
                    ViewName = "_TablaCostosPartial",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<IEnumerable<CostoEjecutadoDto>>(
                        ViewData,
                        lista
                    )
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar gastos del proyecto: {ex.Message}");
                return new PartialViewResult
                {
                    ViewName = "Shared/_TablaCostosPartial",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<IEnumerable<CostoEjecutadoDto>>(
                        ViewData,
                        new List<CostoEjecutadoDto>()
                    )
                };
            }
        }
        public async Task<IActionResult> OnPostEditarCostoAsync()
        {
            // Validar que exista el Id del costo
            if (CostoSeleccionado.IdCosto <= 0)
            {
                TempData["ErrorMessage"] = "El identificador del costo ejecutado no es válido.";
                TempData["TabActiva"] = "GastosEjecutados";
                return Page();
            }

            //OnPostCrearDocumentoSimpleAsync();

            var resultado = await _FinanzasService.EditarCostoEjecutadoAsync(CostoSeleccionado);

            if (resultado)
            {
                TempData["SuccessMessage"] = "Costo ejecutado actualizado correctamente.";
                ModelState.Clear();
                CostoSeleccionado = new CostoEjecutadoDto();
            }
            else
            {
                TempData["ErrorMessage"] = "Error al actualizar el costo ejecutado.";
            }

            // Recargar datos
            CostosEjecutados = (await _FinanzasService.ListarCostosEjecutadosAsync()).ToList();
            Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
            Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
            TempData["TabActiva"] = "GastosEjecutados";

            return Page();
        }
        public async Task<IActionResult> OnPostEliminarCostoAsync()
        {
            if (CostoSeleccionado.IdCosto <= 0)
            {
                TempData["ErrorMessage"] = "Identificador de costo inválido.";
                TempData["TabActiva"] = "GastosEjecutados";
                return Page();
            }

            var resultado = await _FinanzasService.EliminarCostoEjecutadoAsync(CostoSeleccionado.IdCosto);

            if (resultado)
            {
                TempData["SuccessMessage"] = "Costo ejecutado eliminado correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar el costo ejecutado.";
            }

            // Recargar datos
            CostosEjecutados = (await _FinanzasService.ListarCostosEjecutadosAsync()).ToList();
            Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
            Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
            TempData["TabActiva"] = "GastosEjecutados";

            return Page();
        }

        public async Task<bool> CrearDocumentoSimple()
        {
            bool resultado = false;
            // Validar campos requeridos
            if (ArchivoSimple == null || string.IsNullOrWhiteSpace(NombreDocumentoSimple))
            {
                TempData["ErrorMessage"] = "Complete todos los campos requeridos.";
                resultado = false;
            }
            else
            {

                try
                {
                    // 1. Validar archivo
                    if (!ValidarArchivo(ArchivoSimple, out string mensajeError))
                    {
                        TempData["ErrorMessage"] = mensajeError;
                        resultado = false;

                    }
                    else
                    {
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
                            ProyectoId = CostoSeleccionado.ProyectoId,
                            NombreDocumento = NombreDocumentoSimple,
                            CategoriaDocumento = CostoSeleccionado.CategoriaGasto,
                            Descripcion = CostoSeleccionado.Descripcion,
                            NombreArchivoOriginal = ArchivoSimple.FileName,
                            RutaBlobCompleta = blobUrl,
                            TipoArchivo = ArchivoSimple.ContentType,
                            TamanoBytes = ArchivoSimple.Length,
                            CreadoPor = User.FindFirstValue(ClaimTypes.NameIdentifier)!
                        };

                        await _DocumentosService.CrearDocumentoSimpleAsync(dto);
                        CostoSeleccionado.RutaComprobante = blobUrl;


                        TempData["SuccessMessage"] = $"Documento '{NombreDocumentoSimple}' creado correctamente.";
                        TempData["TabActiva"] = "Documentos";
                        resultado = true;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error al crear documento: {ex.Message}";
                    resultado = false;
                }
            }

            return resultado;
        }

        public async Task<bool> CrearDocumentoSimplePagoProveedor()
        {
            bool resultado = false;
            // Validar campos requeridos
            if (ArchivoSimplePagoProveedor == null || string.IsNullOrWhiteSpace(NombreDocumentoSimple))
            {
                TempData["ErrorMessage"] = "Complete todos los campos requeridos.";
                resultado = false;
            }
            else
            {

                try
                {
                    // 1. Validar archivo
                    if (!ValidarArchivo(ArchivoSimplePagoProveedor, out string mensajeError))
                    {
                        TempData["ErrorMessage"] = mensajeError;
                        resultado = false;

                    }
                    else
                    {
                        // 3. Subir archivo a Blob Storage
                        string blobUrl;
                        using (var stream = ArchivoSimplePagoProveedor.OpenReadStream())
                        {
                            blobUrl = await _blobStorageService.SubirArchivoAsync(
                                stream,
                                ArchivoSimplePagoProveedor.FileName,
                                ArchivoSimplePagoProveedor.ContentType
                            );
                        }

                        // 4. Crear documento simple en BD
                        var dto = new CrearDocumentoSimpleDto
                        {
                            ProyectoId = PagoSeleccionado.ProyectoId,
                            NombreDocumento = NombreDocumentoSimple,
                            CategoriaDocumento = "Factura",
                            Descripcion = PagoSeleccionado.Descripcion,
                            NombreArchivoOriginal = ArchivoSimplePagoProveedor.FileName,
                            RutaBlobCompleta = blobUrl,
                            TipoArchivo = ArchivoSimplePagoProveedor.ContentType,
                            TamanoBytes = ArchivoSimplePagoProveedor.Length,
                            CreadoPor = User.FindFirstValue(ClaimTypes.NameIdentifier)!
                        };

                        await _DocumentosService.CrearDocumentoSimpleAsync(dto);
                        PagoSeleccionado.RutaComprobante = blobUrl;

                        TempData["SuccessMessage"] = $"Documento '{NombreDocumentoSimple}' creado correctamente.";
                        TempData["TabActiva"] = "Documentos";
                        resultado = true;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error al crear documento: {ex.Message}";
                    resultado = false;
                }
            }

            return resultado;
        }

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

        //Ver documento simple en pestaña nueva
        public async Task<IActionResult> OnGetVerDocumentoAsync(string rutaComprobante)
        {
            try
            {

                if (rutaComprobante.IsNullOrEmpty())
                {
                    TempData["ErrorMessage"] = "Documento no encontrado.";
                    // Recargar datos
                    CostosEjecutados = (await _FinanzasService.ListarCostosEjecutadosAsync()).ToList();
                    Proyectos = (await _FinanzasService.ListarProyectosAsync()).ToList();
                    PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
                    Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
                    TempData["TabActiva"] = "GastosEjecutados";

                    return Page();
                }


                // 2. Generar URL temporal con SAS Token (válida por 60 minutos)
                var urlTemporal = await _blobStorageService.ObtenerUrlTemporalAsync(
                    rutaComprobante,
                    duracionMinutos: 60
                );

                // 3. Redirigir a la URL temporal
                return Redirect(urlTemporal);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al acceder al documento, busque si existe o si ya fue eliminado";
                return RedirectToPage();
            }
        }
    }
}