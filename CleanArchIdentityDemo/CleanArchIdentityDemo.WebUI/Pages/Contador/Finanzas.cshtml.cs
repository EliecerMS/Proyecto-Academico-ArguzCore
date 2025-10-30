using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Contador
{
    [Authorize(Roles = "Contador")]
    public class FinanzasModel : PageModel
    {
        private readonly IFinanzasService _FinanzasService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IProyectoService _ProyectoService;

        public FinanzasModel(IFinanzasService finanzasService, IUserService userService, UserManager<ApplicationUser> userManager, IProyectoService proyectoService)
        {
            _FinanzasService = finanzasService;
            _UserService = userService;
            _UserManager = userManager;
            _ProyectoService = proyectoService;
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

        [BindProperty]
        public IFormFile? ArchivoComprobante { get; set; }

        public async Task OnGetAsync()
        {
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
            Proveedores = (await _FinanzasService.ListarProveedoresAsync()).ToList();
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

            // Registrar el pago
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
        public async Task<IActionResult> OnPostRegistrarCostoAsync(IFormFile? ArchivoComprobante)
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

            // Guardar archivo comprobante si se subió
            if (ArchivoComprobante != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{ArchivoComprobante.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await ArchivoComprobante.CopyToAsync(fileStream);

                // Asignar ruta relativa
                CostoSeleccionado.RutaComprobante = $"/comprobantes/{uniqueFileName}";
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
        public async Task<IActionResult> OnPostEditarCostoAsync(IFormFile? ArchivoComprobante)
        {
            // Validar que exista el Id del costo
            if (CostoSeleccionado.IdCosto <= 0)
            {
                TempData["ErrorMessage"] = "El identificador del costo ejecutado no es válido.";
                TempData["TabActiva"] = "GastosEjecutados";
                return Page();
            }

            // Guardar nuevo comprobante si se subió uno
            if (ArchivoComprobante != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{ArchivoComprobante.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await ArchivoComprobante.CopyToAsync(fileStream);

                // Actualizar ruta
                CostoSeleccionado.RutaComprobante = $"/comprobantes/{uniqueFileName}";
            }

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
    }
}