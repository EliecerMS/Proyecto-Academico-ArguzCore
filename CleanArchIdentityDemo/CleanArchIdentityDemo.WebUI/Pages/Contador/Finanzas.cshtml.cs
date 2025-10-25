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

        public FinanzasModel(IFinanzasService finanzasService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _FinanzasService = finanzasService;
            _UserService = userService;
            _UserManager = userManager;
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
    }
}