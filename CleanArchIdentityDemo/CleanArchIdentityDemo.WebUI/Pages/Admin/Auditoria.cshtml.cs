using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data;

namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class AuditoriaModel : PageModel
    {
        private readonly IAuditoriaService _auditoriaService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _UserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public AuditoriaModel(IAuditoriaService auditoriaService, IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _auditoriaService = auditoriaService;
            _UserService = userService;
            _UserManager = userManager;
        }

        //Lista de registros para mostrar en la vista
        public List<AuditoriaDto> RegistrosAuditoria { get; set; } = new();
        public List<AccesoModuloDto> RegistrosAccesosModulo { get; set; } = new();
        public List<ApplicationUser> Usuarios { get; set; } = new();

        public List<string> FiltroUsuarios { get; set; } = new();
        public List<string> Acciones { get; set; } = new();
        public List<string> Modulos { get; set; } = new();
        //para paginacion

        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int ElementosPorPagina { get; set; } = 20;

        public ResultadoPaginado<AuditoriaDto> Auditorias { get; set; }

        // Cargar los registros cuando se abre la página

        public async Task OnGetAsync()
        {
            Auditorias = await _auditoriaService.MostrarRegistrosAsync(PaginaActual, ElementosPorPagina);

            RegistrosAccesosModulo = await _auditoriaService.ObtenerAccesosPorModuloAsync(null, null);

            //Registra el acceso de los usuarios y lo guarda en la tabla de auditoria
            await _auditoriaService.RegistrarAccesoAsync("Auditoria");

        }

        public async Task<IActionResult> OnGetFiltrarAccesosAsync(string modulo, string usuario)
        {
            var resultados = await _auditoriaService.ObtenerAccesosPorModuloAsync(modulo, usuario);

            return Partial("_TablaAccesosModuloPartial", resultados);
        }

        public async Task<FileResult> OnGetDescargarPdfAuditoriaAsync(
    string usuarioId, string accion, string modulo, DateTime? desde, DateTime? hasta)
        {
            var datos = await _auditoriaService.ObtenerAccesosPorModuloAsync(modulo, usuarioId);

            if (!string.IsNullOrEmpty(usuarioId))
                datos = datos.Where(a => a.UsuarioId == usuarioId).ToList();

            if (!string.IsNullOrEmpty(modulo))
                datos = datos.Where(a => a.Modulo == modulo).ToList();

            if (datos == null || !datos.Any())
                throw new Exception("No hay datos para generar el PDF");

            var pdf = await _auditoriaService.GenerarReportePdfAsync(datos);

            return File(pdf, "application/pdf", "ReporteAccesos.pdf");
        }







        public async Task<PartialViewResult> OnGetFiltrarAsync(string usuario, string accion, string modulo, DateTime? desde, DateTime? hasta)
        {
            var resultado = await _auditoriaService.MostrarRegistrosAsync(PaginaActual = 1, ElementosPorPagina);

            // Extraer la lista REAL
            var auditorias = resultado.Items.AsQueryable();

            // Filtros por usuario, acción y módulo
            if (!string.IsNullOrEmpty(usuario))
                auditorias = auditorias.Where(a => a.NombreUsuario == usuario);
            if (!string.IsNullOrEmpty(accion))
                auditorias = auditorias.Where(a => a.Accion == accion);
            if (!string.IsNullOrEmpty(modulo))
                auditorias = auditorias.Where(a => a.Modulo == modulo);
            // Filtros por fecha
            if (desde.HasValue)
                auditorias = auditorias.Where(a => a.FechaHora >= desde.Value);
            if (hasta.HasValue)
                auditorias = auditorias.Where(a => a.FechaHora <= hasta.Value);
            // Cargar listas para los select (evita duplicados)
            FiltroUsuarios = auditorias
                .Where(a => !string.IsNullOrEmpty(a.NombreUsuario))
                .Select(a => a.NombreUsuario)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Actualizar el resultado paginado con la lista filtrada
            resultado.Items = auditorias.ToList();

            // Retornar la tabla parcial actualizada
            return new PartialViewResult
            {
                ViewName = "_TablaAuditoriaPartial",
                ViewData = new ViewDataDictionary<ResultadoPaginado<AuditoriaDto>>(ViewData, resultado)
            };
        }

        public async Task<FileResult> OnGetDescargarExcelAsync(string usuario, string accion, string modulo, DateTime? desde, DateTime? hasta)
        {
            var resultado = await _auditoriaService.MostrarRegistrosAsync(PaginaActual, ElementosPorPagina);

            // Extraer la lista REAL
            var auditorias = resultado.Items.AsQueryable();

            // Filtros iguales al método Filtrar
            if (!string.IsNullOrEmpty(usuario))
                auditorias = auditorias.Where(a => a.NombreUsuario == usuario);
            if (!string.IsNullOrEmpty(accion))
                auditorias = auditorias.Where(a => a.Accion == accion);
            if (!string.IsNullOrEmpty(modulo))
                auditorias = auditorias.Where(a => a.Modulo == modulo);
            if (desde.HasValue)
                auditorias = auditorias.Where(a => a.FechaHora >= desde.Value);
            if (hasta.HasValue)
                auditorias = auditorias.Where(a => a.FechaHora <= hasta.Value);

            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Auditoría");

            // Encabezados
            hoja.Cell(1, 1).Value = "Usuario";
            hoja.Cell(1, 2).Value = "Acción";
            hoja.Cell(1, 3).Value = "Módulo";
            hoja.Cell(1, 4).Value = "Dato Anterior";
            hoja.Cell(1, 5).Value = "Dato Nuevo";
            hoja.Cell(1, 6).Value = "Fecha";

            int fila = 2;
            foreach (var a in auditorias)
            {
                hoja.Cell(fila, 1).Value = a.NombreUsuario;
                hoja.Cell(fila, 2).Value = a.Accion;
                hoja.Cell(fila, 3).Value = a.Modulo;
                hoja.Cell(fila, 4).Value = a.DatoAnterior;
                hoja.Cell(fila, 5).Value = a.DatoNuevo;
                hoja.Cell(fila, 6).Value = a.FechaHora.ToString("g");
                fila++;
            }

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteAuditoria.xlsx");
        }


    }
}
