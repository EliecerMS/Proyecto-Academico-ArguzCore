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

        [BindProperty]
        public int PagoId { get; set; } //almacenara el Id del pago seleccionado para ver el recibo

        public async Task OnGetAsync()
        {
            PagosProveedores = (await _FinanzasService.ListarPagosProveedoresAsync()).ToList();
        }
        public async Task<IActionResult> OnPostVerReciboAsync()
        {
            // GenerarComprobantePago es un método del servicio que genera el comprobante de pago en formato PDF y lo devuelve como un arreglo de bytes
            var reciboBytes = _FinanzasService.GenerarComprobante(PagoId);

            // Retornar el archivo en PDF
            return File(reciboBytes, "application/pdf");
        }
    }
}
