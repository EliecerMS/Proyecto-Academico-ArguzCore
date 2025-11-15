using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Bodeguero
{
    [Authorize(Roles = "Bodeguero,Administrador")]
    public class MaterialesModel : PageModel
    {
        private readonly IMaterialesService _MaterialesService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public MaterialesModel(IMaterialesService materialesService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _MaterialesService = materialesService;
            _UserService = userService;
            _UserManager = userManager;
        }



        public List<MaterialDto> Materiales { get; set; } = new();

        public List<MaterialSolicitadoDto> MaterialesSolicitados { get; set; } = new();

        // Lista de proveedores como DTO
        public List<ProveedorMaterialDto> Proveedores { get; set; } = new();

        [BindProperty]
        public MaterialDto NuevoMaterial { get; set; }

        [BindProperty]
        public MaterialDto EditadoMaterial { get; set; }

        [BindProperty]
        public MaterialDto materialEditado { get; set; }


        public async Task OnGetAsync()
        {
            Materiales = (await _MaterialesService.MostrarMaterialesAsync()).ToList();
            MaterialesSolicitados = (await _MaterialesService.MostrarMaterialesSolicitadosAsync()).ToList();

            var proveedoresDto = await _MaterialesService.GetProveedoresAsync();
            Proveedores = proveedoresDto.ToList();
        }

        public async Task<IActionResult> OnPostRegistrarMaterial()
        {
            if (!ModelState.IsValid)
            {


                TempData["ErrorMessage"] = "Hay errores en el formulario. Verifica los campos.";
                TempData["TabActiva"] = "MaterialesBodega";
                return Page();
            }


            try
            {

                await _MaterialesService.RegistrarMaterialAsync(NuevoMaterial);

                TempData["SuccessMessage"] = "Material registrado correctamente.";
                TempData["TabActiva"] = "MaterialesBodega";

                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                TempData["TabActiva"] = "MaterialesBodega";
                return RedirectToPage();
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el material.";
                TempData["TabActiva"] = "MaterialesBodega";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEliminarMaterialAsync(int IdMaterial)
        {
            try
            {
                await _MaterialesService.EliminarMaterialAsync(IdMaterial);
                TempData["SuccessMessage"] = "Material eliminado correctamente.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el material.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(MaterialDto EditadoMaterial)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario de edición. Verifica los campos.";
                TempData["TabActiva"] = "MaterialesBodega";
                return RedirectToPage();
            }
            try
            {
                await _MaterialesService.EditarMaterialAsync(EditadoMaterial);
                TempData["SuccessMessage"] = "Material editado correctamente.";
                TempData["TabActiva"] = "MaterialesBodega";
                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                TempData["TabActiva"] = "MaterialesBodega";
                return RedirectToPage();
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al editar el material.";
                TempData["TabActiva"] = "MaterialesBodega";
                return RedirectToPage();
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetObtenerSolicitudAsync(int id)
        {
            var solicitud = await _MaterialesService.MostrarSolicitudPorIdAsync(id);

            if (solicitud == null)
                return new JsonResult(new { error = "Solicitud no encontrada" });

            return new JsonResult(solicitud);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> OnPostAceptarSolicitudAsync(int IdSolicitud, string Observaciones)
        {
            // Lógica para aceptar solicitud
            var resultado = await _MaterialesService.AceptarSolicitudAsync(IdSolicitud, Observaciones);

            // Validar respuesta del servicio
            if (resultado == "EXCEDE")
                return new JsonResult(new { excede = true });

            TempData["SuccessMessage"] = "Solicitud aceptada correctamente.";
            TempData["TabActiva"] = "SolicitudesMaterial";
            return new JsonResult(new { excede = false });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> OnPostRechazarSolicitudAsync(int idSolicitud, string Observaciones)
        {
            var solicitud = await _MaterialesService.RechazarSolicitudAsync(idSolicitud, Observaciones);

            // Validar respuesta del servicio
            if (solicitud == "EXCEDE")
                return new JsonResult(new { excede = true });

            TempData["WarningMessage"] = "Solicitud rechazada.";
            TempData["TabActiva"] = "SolicitudesMaterial";
            return new JsonResult(new { ok = true });
        }
    }


}
