using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuditoriaService(
                                ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }


        public Task<byte[]> GenerarReportePdfAsync(List<AccesoModuloDto> datos, string titulo = "Reporte de Accesos a Módulos")
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Text(titulo)
                        .FontSize(20)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // Modulo
                                columns.RelativeColumn(); // Usuario
                                columns.ConstantColumn(90); // Cantidad
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Módulo");
                                header.Cell().Element(HeaderStyle).Text("Usuario");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Accesos");

                                static IContainer HeaderStyle(IContainer c) =>
                                    c.Background(Colors.Grey.Lighten3)
                                     .Padding(5)
                                     .Border(1)
                                     .BorderColor(Colors.Grey.Medium);
                            });

                            foreach (var item in datos)
                            {
                                table.Cell().Element(BodyStyle).Text(item.Modulo ?? "");
                                table.Cell().Element(BodyStyle).Text(item.NombreUsuario ?? "");
                                table.Cell().Element(BodyStyle).AlignRight().Text(item.Cantidad.ToString());


                                static IContainer BodyStyle(IContainer c) =>
                                    c.Padding(5)
                                     .BorderBottom(1)
                                     .BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        col.Item().Text($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(10)
                            .AlignRight();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                });
            });

            var bytes = doc.GeneratePdf();
            return Task.FromResult(bytes);
        }



        public async Task<ResultadoPaginado<AuditoriaDto>> MostrarRegistrosAsync(int numeroPagina = 1, int tamañoPagina = 20)
        {
            // 1.IMPORTANTE: Contar primero ANTES de paginar
            var totalRegistros = await _context.AuditoriaAcciones.CountAsync();

            // 2. Obtener solo los registros de la página actual
            var auditorias = await _context.AuditoriaAcciones
                .Where(a => a.Accion != "Acceso") // Placeholder para posibles filtros futuros
                .OrderByDescending(a => a.FechaHora)
                .Skip((numeroPagina - 1) * tamañoPagina)  // Saltar páginas anteriores
                .Take(tamañoPagina)                       // Tomar solo los de esta página
                .ToListAsync();

            // 3. Obtener IDs únicos de usuarios (solo los de esta página)
            var usuarioIds = auditorias
                .Where(a => !string.IsNullOrEmpty(a.UsuarioId))
                .Select(a => a.UsuarioId)
                .Distinct()
                .ToList();

            // 4. Obtener usuarios en UNA SOLA consulta (no en loop)
            var usuarios = await _context.Users
                .Where(u => usuarioIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            // 5. Crear diccionario para lookup rápido
            var usuariosDic = usuarios.ToDictionary(u => u.Id, u => u.UserName);

            // 6. Mapear a DTOs
            var result = auditorias.Select(a => new AuditoriaDto
            {
                IdAuditoria = a.IdAuditoria,
                UsuarioId = a.UsuarioId,
                NombreUsuario = string.IsNullOrEmpty(a.UsuarioId)
                    ? "Sistema"
                    : usuariosDic.GetValueOrDefault(a.UsuarioId, "Usuario desconocido"),
                Modulo = a.Modulo,
                Accion = a.Accion,
                DatoAnterior = a.DatoAnterior,
                DatoNuevo = a.DatoNuevo,
                FechaHora = a.FechaHora
            }).ToList();

            // 7. Retornar resultado paginado
            return new ResultadoPaginado<AuditoriaDto>(
                result,
                totalRegistros,
                numeroPagina,
                tamañoPagina
            );
        }

        public async Task<List<AccesoModuloDto>> ObtenerAccesosPorModuloAsync(string modulo, string usuarioId)
        {
            var query = _context.AuditoriaAcciones
                .Where(a => a.Accion == "Acceso")
                .AsQueryable();

            if (!string.IsNullOrEmpty(modulo))
                query = query.Where(a => a.Modulo == modulo);

            if (!string.IsNullOrEmpty(usuarioId))
                query = query.Where(a => a.UsuarioId == usuarioId);

            var result = await query
                .GroupBy(a => new { a.Modulo, a.UsuarioId })
                .Select(g => new AccesoModuloDto
                {
                    UsuarioId = g.Key.UsuarioId,
                    NombreUsuario = _context.Users
                                .Where(u => u.Id == g.Key.UsuarioId)
                                .Select(u => u.UserName)
                                .FirstOrDefault(),
                    Modulo = g.Key.Modulo,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return result;
        }



        public async Task<List<AccesoModuloDto>> ObtenerAccesosPorUsuarioAsync(string usuarioId)
        {
            var user = await _userManager.FindByIdAsync(usuarioId);

            return await _context.AuditoriaAcciones
            .Where(a => a.Accion == "Acceso" && a.UsuarioId == usuarioId)
            .GroupBy(a => a.Modulo)
            .Select(g => new AccesoModuloDto
            {
                UsuarioId = usuarioId,
                NombreUsuario = user.UserName ?? "Desconocido",
                Modulo = g.Key,
                Cantidad = g.Count()
            })
            .ToListAsync();
        }

        public async Task RegistrarAccesoAsync(string modulo)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? "Sistema";

            var registro = new AuditoriaAccion
            {
                UsuarioId = userId,
                Modulo = modulo,
                Accion = "Acceso",
                FechaHora = DateTime.Now
            };

            _context.AuditoriaAcciones.Add(registro);
            await _context.SaveChangesAsync();
        }

    }
}
