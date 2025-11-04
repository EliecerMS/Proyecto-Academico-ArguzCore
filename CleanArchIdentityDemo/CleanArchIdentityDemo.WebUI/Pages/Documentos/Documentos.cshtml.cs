using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.WebUI.Pages.Documentos
{
    [Authorize(Roles = "Administrador,SupervisorProyectos,Contador")]
    public class DocumentosModel : PageModel
    {
        private readonly IDocumentosService _DocumentosService;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IContratoService _ContratoService;
        private readonly ApplicationDbContext _context;
        public List<ContratoDto> Contratos { get; private set; } = new();
        public List<ProyectoDto> ProyectosDisponibles { get; private set; } = new();
        [BindProperty]
        public ContratoDto NuevoContrato { get; set; } = new();
        [BindProperty]
        public ContratoDto EditarContrato { get; set; } = new();


        public DocumentosModel(
            IDocumentosService documentosService,
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            IContratoService contratoService,
            ApplicationDbContext context)
        {
            _DocumentosService = documentosService;
            _UserService = userService;
            _UserManager = userManager;
            _ContratoService = contratoService;
            _context = context;
        }

        public async Task OnGet()
        {
            Contratos = (await _ContratoService.GetAllAsync()).ToList();

            ProyectosDisponibles = await _context.Proyectos
                .Select(p => new ProyectoDto { IdProyecto = p.IdProyecto, Nombre = p.Nombre })
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }



        public async Task<IActionResult> OnPostRegistrarContratoAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevoContrato.Descripcion))
                ModelState.AddModelError(nameof(NuevoContrato.Descripcion), "El nombre del empleado es requerido.");

            if (NuevoContrato.ProyectoId <= 0)
                ModelState.AddModelError(nameof(NuevoContrato.ProyectoId), "Debe seleccionar un proyecto.");

            if (NuevoContrato.FechaFin < NuevoContrato.FechaInicio)
                ModelState.AddModelError(nameof(NuevoContrato.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");

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

    }
}

