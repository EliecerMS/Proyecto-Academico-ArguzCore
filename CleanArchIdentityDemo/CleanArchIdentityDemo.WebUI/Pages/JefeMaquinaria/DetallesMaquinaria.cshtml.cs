using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.WebUI.Pages.JefeMaquinaria
{
    [Authorize(Roles = "JefeMaquinaria")]
    public class DetallesMaquinariaModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEquipoService _equipoService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetallesMaquinariaModel(
            ApplicationDbContext context,
            IEquipoService equipoService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _equipoService = equipoService;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public MaquinariaDto? Maquinaria { get; set; } // Detalles de la maquinaria
        public List<MantenimientoMaquinariaDto> Mantenimientos { get; set; } = new(); // Historial de mantenimientos
        public MaquinariaProyectoDto? ProyectoAsignado { get; set; } // Proyecto asignado actualmente
        public List<ProyectoDto> ProyectosDisponibles { get; set; } = new(); // Lista de proyectos para asignar
        public bool HayMantenimientoActivo { get; set; }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            //Obtener maquinaria
            Maquinaria = await _equipoService.GetByIdAsync(id);
            if (Maquinaria == null)
            {
                TempData["ErrorMessage"] = "No se encontró la maquinaria solicitada.";
                return RedirectToPage("/Error");
            }

            //Obtener historial de mantenimientos
            Mantenimientos = (await _equipoService.GetMantenimientosPorMaquinariaAsync(id))
                .OrderByDescending(m => m.FechaProgramada)
                .ToList();

            //Obtener el proyecto asignado (si lo tiene)
            ProyectoAsignado = await _equipoService.GetProyectoAsignadoAsync(id);

            //Obtener lista de proyectos disponibles
            ProyectosDisponibles = await _context.Proyectos
                .Select(p => new ProyectoDto
                {
                    IdProyecto = p.IdProyecto,
                    Nombre = p.Nombre
                })
                .ToListAsync();

            // Validación: ¿hay algún mantenimiento activo?
            HayMantenimientoActivo = Mantenimientos.Any(m => m.Estado == "Ejecución" || m.FechaCompletado == DateTime.MinValue);

            return Page();
        }

        // Asignar proyecto a una maquinaria
        public async Task<IActionResult> OnPostAsignarProyectoAsync(int MaquinariaId, int ProyectoId)
        {
            if (ProyectoId == 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un proyecto antes de continuar.";
                return RedirectToPage(new { id = MaquinariaId });
            }

            var resultado = await _equipoService.AsignarProyectoAsync(MaquinariaId, ProyectoId);

            if (resultado)
            {
                TempData["SuccessMessage"] = "El proyecto fue asignado correctamente a la maquinaria.";
            }
            else
            {
                TempData["ErrorMessage"] = "No fue posible asignar el proyecto. Verifique si ya tiene uno asignado.";
            }

            return RedirectToPage(new { id = MaquinariaId });
        }

        // Handler para desasignar un proyecto de la maquinaria
        public async Task<IActionResult> OnPostDesasignarProyectoAsync(int MaquinariaId)
        {
            var resultado = await _equipoService.DesasignarProyectoAsync(MaquinariaId);

            if (resultado)
            {
                TempData["SuccessMessage"] = "Proyecto desasignado correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No fue posible desasignar el proyecto.";
            }

            return await OnGetAsync(MaquinariaId);// Recargar los datos de la página para que Maquinaria, ProyectoAsignado y demás no queden null
        }

        // Iniciar mantenimiento de la maquinaria
        public async Task<IActionResult> OnPostIniciarMantenimientoAsync(int MaquinariaId)
        {
            var resultado = await _equipoService.IniciarMantenimientoAsync(MaquinariaId);

            if (resultado)
            {
                TempData["SuccessMessage"] = "Mantenimiento iniciado correctamente. La maquinaria ahora está en estado 'Mantenimiento'.";
            }
            else
            {
                TempData["ErrorMessage"] = "No fue posible iniciar el mantenimiento. Verifique que la maquinaria exista.";
            }

            return RedirectToPage(new { id = MaquinariaId });
        }

        // Finalizar mantenimiento
        public async Task<IActionResult> OnPostFinalizarMantenimientoAsync(int MaquinariaId)
        {
            var resultado = await _equipoService.FinalizarMantenimientoAsync(MaquinariaId);

            if (resultado)
                TempData["SuccessMessage"] = "Mantenimiento finalizado correctamente.";
            else
                TempData["ErrorMessage"] = "No se pudo finalizar el mantenimiento.";

            return RedirectToPage(new { id = MaquinariaId });
        }

    }
}
