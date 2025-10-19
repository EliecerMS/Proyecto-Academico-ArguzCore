using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.WebUI.Pages.Bodeguero
{
    [Authorize(Roles = "Bodeguero")]
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

        //public class Material
        //{
        //    public int IdMaterial { get; set; }
        //    [Required(ErrorMessage = "El nombre del material es obligatorio.")]
        //    public string NombreMaterial { get; set; }
        //    [Required(ErrorMessage = "El tipo de material es obligatorio.")]
        //    public string Tipo { get; set; }
        //    public string Descripcion { get; set; }
        //    [Required(ErrorMessage = "La cantidad disponible es obligatoria.")]
        //    public int CantidadDisponible { get; set; }

        //    [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        //    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor válido.")]
        //    public int ProveedorId { get; set; }

        //}

        public List<MaterialDto> Materiales { get; set; } = new();

        public List<MaterialSolicitadoDto> MaterialesSolicitados { get; set; } = new();

        // Lista de proveedores como DTO
        public List<ProveedorMaterialDto> Proveedores { get; set; } = new();

        [BindProperty]
        public MaterialDto NuevoMaterial { get; set; }

        

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
        public async Task<IActionResult> OnPostRechazarSolicitudAsync(int idSolicitud)
        {
            await _MaterialesService.RechazarSolicitudAsync(idSolicitud);

            TempData["WarningMessage"] = "Solicitud rechazada.";
            TempData["TabActiva"] = "SolicitudesMaterial";
            return new JsonResult(new { ok = true });
        }
    }

 
}
