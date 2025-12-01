using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Services;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;


namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    public class AnaliticaModel : PageModel
    {
        private readonly IProyectoService _proyectoService;
        private readonly ApplicationDbContext _context;
        private readonly IAnaliticaService _analiticaService;
        private readonly IAuditoriaService _auditoriaService;

        public AnaliticaModel(IProyectoService proyectoService,
                              ApplicationDbContext context,
                              IAuditoriaService auditoria,
                              IAnaliticaService analiticaService)
        {
            _proyectoService = proyectoService;
            _context = context;
            _analiticaService = analiticaService;
            _auditoriaService = auditoria;
        }


        // ====== Filtros seleccionados en la vista (se pasan por querystring) ======
        [BindProperty(SupportsGet = true)]
        public int? ProyectoAId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ProyectoBId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MesA { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MesB { get; set; }

        // ====== Datos para poblar los combos ======
        public List<SelectListItem> Proyectos { get; set; } = new();
        public List<SelectListItem> MesesProyectoA { get; set; } = new();
        public List<SelectListItem> MesesProyectoB { get; set; } = new();

        public ComparacionVm? Comparacion { get; set; }

        public async Task OnGetAsync()
        {
            // Cargar lista de proyectos
            await CargarProyectosAsync();

            // Cargar lista de meses para cada proyecto
            if (ProyectoAId.HasValue)
                MesesProyectoA = await CargarMesesProyectoAsync(ProyectoAId.Value);

            if (ProyectoBId.HasValue)
                MesesProyectoB = await CargarMesesProyectoAsync(ProyectoBId.Value);

            // Si ya se seleccionaron ambos proyectos, calculamos la comparación
            if (ProyectoAId.HasValue && ProyectoBId.HasValue)
            {
                Comparacion = await ConstruirComparacionAsync();
            }

            //Registra el acceso de los usuarios y lo guarda en la tabla de auditoria
            await _auditoriaService.RegistrarAccesoAsync("Analitica");
        }

        private async Task CargarProyectosAsync()
        {
            var proyectos = await _proyectoService.MostrarProyectosGeneralAsync();

            Proyectos = proyectos
                .Select(p => new SelectListItem
                {
                    Value = p.IdProyecto.ToString(),
                    Text = p.Nombre
                })
                .ToList();
        }
        

        private async Task<List<SelectListItem>> CargarMesesProyectoAsync(int idProyecto)
        {
            var proyecto = await _context.Proyectos
                .FirstOrDefaultAsync(p => p.IdProyecto == idProyecto);

            if (proyecto == null || !proyecto.FechaInicioPropuesta.HasValue)
                return new List<SelectListItem>();

            var inicio = proyecto.FechaInicioPropuesta.Value.Date;
            var fin = proyecto.FechaFinalPropuesta.Year > 2001
                ? proyecto.FechaFinalPropuesta.Date
                : DateTime.Today;

            if (fin < inicio)
                return new List<SelectListItem>();

            int meses = ((fin.Year - inicio.Year) * 12) + fin.Month - inicio.Month + 1;
            if (meses < 1) meses = 1;

            var lista = new List<SelectListItem>();
            for (int i = 1; i <= meses; i++)
            {
                lista.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = $"Mes {i}"
                });
            }

            return lista;
        }

        private async Task<ComparacionVm> ConstruirComparacionAsync()
        {
            // Cargamos ambos proyectos con Estado y Tareas
            var proyectoA = await _context.Proyectos
                .Include(p => p.EstadoProyecto)
                .Include(p => p.Tareas)
                .FirstOrDefaultAsync(p => p.IdProyecto == ProyectoAId);

            var proyectoB = await _context.Proyectos
                .Include(p => p.EstadoProyecto)
                .Include(p => p.Tareas)
                .FirstOrDefaultAsync(p => p.IdProyecto == ProyectoBId);

            if (proyectoA == null || proyectoB == null)
                throw new Exception("No se encontraron uno o ambos proyectos.");

            // ================== NORMALIZAR MESES ==================
            int mesesMaxA = CalcularMesesTotalesLocal(proyectoA);
            int mesesMaxB = CalcularMesesTotalesLocal(proyectoB);

            int mesA = MesA ?? mesesMaxA;
            int mesB = MesB ?? mesesMaxB;

            if (mesA < 1) mesA = 1;
            if (mesA > mesesMaxA) mesA = mesesMaxA;

            if (mesB < 1) mesB = 1;
            if (mesB > mesesMaxB) mesB = mesesMaxB;

            // ================= PROYECTO A =================
            int avanceA = _proyectoService.RecalculoPorcentajeAvancePorMes(proyectoA, mesA);
            decimal costoEjecutadoA = await _proyectoService.CostosEjecutadosPorMesAsync(proyectoA.IdProyecto, mesA);
            decimal desviacionA = await _proyectoService.DesviacionPorMesAsync(proyectoA.IdProyecto, mesA);

            decimal presupuestoA = proyectoA.Presupuesto;
            decimal porcentajeEjecutadoA = presupuestoA == 0
                ? 0
                : Math.Round((costoEjecutadoA / presupuestoA) * 100, 2);

            decimal porcentajeDesviacionA = presupuestoA == 0
                ? 0
                : Math.Round((desviacionA / presupuestoA) * 100, 2);

            string riesgoA = CalcularRiesgo(porcentajeDesviacionA);

            // ================= PROYECTO B =================
            int avanceB = _proyectoService.RecalculoPorcentajeAvancePorMes(proyectoB, mesB);
            decimal costoEjecutadoB = await _proyectoService.CostosEjecutadosPorMesAsync(proyectoB.IdProyecto, mesB);
            decimal desviacionB = await _proyectoService.DesviacionPorMesAsync(proyectoB.IdProyecto, mesB);

            decimal presupuestoB = proyectoB.Presupuesto;
            decimal porcentajeEjecutadoB = presupuestoB == 0
                ? 0
                : Math.Round((costoEjecutadoB / presupuestoB) * 100, 2);

            decimal porcentajeDesviacionB = presupuestoB == 0
                ? 0
                : Math.Round((desviacionB / presupuestoB) * 100, 2);

            string riesgoB = CalcularRiesgo(porcentajeDesviacionB);

            return new ComparacionVm
            {
                IdProyectoA = proyectoA.IdProyecto,
                IdProyectoB = proyectoB.IdProyecto,
                NombreProyectoA = proyectoA.Nombre,
                NombreProyectoB = proyectoB.Nombre,
                MesA = mesA,
                MesB = mesB,
                EstadoProyectoA = proyectoA.EstadoProyecto?.NombreEstado ?? "",
                EstadoProyectoB = proyectoB.EstadoProyecto?.NombreEstado ?? "",
                AvanceFisicoA = avanceA,
                AvanceFisicoB = avanceB,
                PresupuestoEjecutadoPorcentajeA = porcentajeEjecutadoA,
                PresupuestoEjecutadoPorcentajeB = porcentajeEjecutadoB,
                DesviacionPorcentajeA = porcentajeDesviacionA,
                DesviacionPorcentajeB = porcentajeDesviacionB,
                RiesgoA = riesgoA,
                RiesgoB = riesgoB
            };
        }

        private int CalcularMesesTotalesLocal(Domain.Entities.Proyecto proyecto)
        {
            if (!proyecto.FechaInicioPropuesta.HasValue)
                return 1;

            var inicio = proyecto.FechaInicioPropuesta.Value.Date;
            var fin = proyecto.FechaFinalPropuesta.Year > 2001
                ? proyecto.FechaFinalPropuesta.Date
                : DateTime.Today;

            if (fin < inicio)
                return 1;

            int meses = ((fin.Year - inicio.Year) * 12) + fin.Month - inicio.Month + 1;
            return meses < 1 ? 1 : meses;
        }

        // Para esto tomé riesgo del proyecto en terminos de desempeño y no tanto de riesgo laboral
        private string CalcularRiesgo(decimal desviacionPorcentaje)
        {
            var desviacionAbs = Math.Abs(desviacionPorcentaje);

            if (desviacionAbs < 5)
                return "Bajo";
            if (desviacionAbs < 15)
                return "Medio";
            return "Alto";
        }

        public async Task<IActionResult> OnPostDescargarPdfAsync()
        {
            // Recalcular la comparativa con los filtros actuales
            await CargarProyectosAsync();

            if (ProyectoAId.HasValue)
                MesesProyectoA = await CargarMesesProyectoAsync(ProyectoAId.Value);

            if (ProyectoBId.HasValue)
                MesesProyectoB = await CargarMesesProyectoAsync(ProyectoBId.Value);

            if (!(ProyectoAId.HasValue && ProyectoBId.HasValue))
            {
                // Si no hay proyectos seleccionados, simplemente recargamos la página
                return RedirectToPage();
            }

            var comparacion = await ConstruirComparacionAsync();

            var dto = new ComparacionAnaliticaDto
            {
                NombreProyectoA = comparacion.NombreProyectoA,
                NombreProyectoB = comparacion.NombreProyectoB,
                MesA = comparacion.MesA,
                MesB = comparacion.MesB,
                EstadoProyectoA = comparacion.EstadoProyectoA,
                EstadoProyectoB = comparacion.EstadoProyectoB,
                AvanceFisicoA = comparacion.AvanceFisicoA,
                AvanceFisicoB = comparacion.AvanceFisicoB,
                PresupuestoEjecutadoPorcentajeA = comparacion.PresupuestoEjecutadoPorcentajeA,
                PresupuestoEjecutadoPorcentajeB = comparacion.PresupuestoEjecutadoPorcentajeB,
                DesviacionPorcentajeA = comparacion.DesviacionPorcentajeA,
                DesviacionPorcentajeB = comparacion.DesviacionPorcentajeB,
                RiesgoA = comparacion.RiesgoA,
                RiesgoB = comparacion.RiesgoB
            };

            var pdfBytes = await _analiticaService.GenerarPdfComparacionAsync(dto);

            return File(pdfBytes, "application/pdf", "ComparativaProyectos.pdf");
        }


        // ViewModel para la tabla
        public class ComparacionVm
        {

            //Registra el acceso de los usuarios y lo guarda en la tabla de auditoria
           
            public int IdProyectoA { get; set; }
            public int IdProyectoB { get; set; }

            public string NombreProyectoA { get; set; } = "";
            public string NombreProyectoB { get; set; } = "";

            public int? MesA { get; set; }
            public int? MesB { get; set; }

            public string EstadoProyectoA { get; set; } = "";
            public string EstadoProyectoB { get; set; } = "";

            public int AvanceFisicoA { get; set; }
            public int AvanceFisicoB { get; set; }

            public decimal PresupuestoEjecutadoPorcentajeA { get; set; }
            public decimal PresupuestoEjecutadoPorcentajeB { get; set; }

            public decimal DesviacionPorcentajeA { get; set; }
            public decimal DesviacionPorcentajeB { get; set; }

            public string RiesgoA { get; set; } = "";
            public string RiesgoB { get; set; } = "";
        }
    }
}

