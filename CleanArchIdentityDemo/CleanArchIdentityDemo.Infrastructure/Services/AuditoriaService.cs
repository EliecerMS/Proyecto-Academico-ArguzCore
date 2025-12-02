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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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



        public async Task<IEnumerable<AuditoriaDto>> MostrarRegistrosAsync()
        {
            var auditorias = await _context.AuditoriaAcciones
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            var result = new List<AuditoriaDto>();

            foreach (var a in auditorias)
            {
                var user = await _userManager.FindByIdAsync(a.UsuarioId ?? "");
                result.Add(new AuditoriaDto
                {
                    IdAuditoria = a.IdAuditoria,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = user?.UserName ?? "Sistema",
                    Modulo = a.Modulo,
                    Accion = a.Accion,
                    DatoAnterior = a.DatoAnterior,
                    DatoNuevo = a.DatoNuevo,
                    FechaHora = a.FechaHora
                });
            }

            return result;
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
