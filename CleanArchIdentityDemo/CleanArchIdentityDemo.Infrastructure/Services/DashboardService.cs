using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProyectoService _proyectoService;

        public DashboardService(ApplicationDbContext context, IProyectoService proyectoService)
        {
            _context = context;
            _proyectoService = proyectoService;
        }

        public async Task<byte[]> GenerarInformeProyectoAsync(int proyectoId)
        {
            // 1. Traer el proyecto de la BD (para nombre, descripción, fechas, presupuesto)
            var proyecto = await _context.Proyectos
                .FirstOrDefaultAsync(p => p.IdProyecto == proyectoId);

            if (proyecto == null)
                throw new Exception("Proyecto no encontrado.");

            // 2. Traer el MISMO DTO que usas en el dashboard
            var proyectosDashboard = await _proyectoService.MostrarProyectosActivosEInactivosAsync();
            var proyectoDash = proyectosDashboard.FirstOrDefault(p => p.IdProyecto == proyectoId);

            if (proyectoDash == null)
                throw new Exception("Proyecto no encontrado en dashboard.");

            // 3. Usar los mismos valores que ve el usuario en la tarjeta
            int avanceFisico = proyectoDash.PorcentajeAvance;                          // mismo 100% del dashboard
            decimal porcentajeFinanciero = proyectoDash.PorcentajePresupuestoEjecutado; // mismo % financiero
            decimal desviacion = proyectoDash.Desviacion;                               // misma desviación

            // 4. Calcular el total ejecutado a partir del porcentaje financiero
            decimal totalEjecutado = 0;
            if (proyectoDash.Presupuesto > 0 && porcentajeFinanciero > 0)
            {
                totalEjecutado = Math.Round(
                    proyectoDash.Presupuesto * (porcentajeFinanciero / 100m), 2
                );
            }

            // Limitar 0–100 para las barras
            int avanceClamped = Math.Clamp(avanceFisico, 0, 100);
            int financieroClamped = (int)Math.Clamp(porcentajeFinanciero, 0, 100);

            // 5. Construir el PDF con esos mismos valores
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text("Informe de avance del proyecto")
                            .FontSize(20).Bold();

                        col.Item().Text($"Proyecto: {proyecto.Nombre}")
                            .FontSize(14);
                        col.Item().Text($"Descripción: {proyecto.Descripcion}");
                        col.Item().Text($"Fecha final propuesta: {proyecto.FechaFinalPropuesta:dd/MM/yyyy}");
                        col.Item().Text($"Presupuesto: ₡{proyecto.Presupuesto:N0}");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Resumen de avance").FontSize(14).Bold();
                        col.Item().Text($"Avance físico: {avanceFisico}%");
                        col.Item().Text($"Presupuesto ejecutado: {porcentajeFinanciero}%");
                        col.Item().Text($"Total ejecutado: ₡{totalEjecutado:N0}");
                        col.Item().Text($"Desviación: ₡{desviacion:N0}");

                        // Barras comparativas 
                        col.Item().Row(row =>
                        {
                            // Barra AVANCE FÍSICO
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Avance físico").Bold();

                                c.Item().Height(12).Row(r =>
                                {
                                    var avance = (float)avanceClamped;

                                    if (avance <= 0f)
                                    {
                                        r.RelativeItem(100f).Background(Colors.Grey.Lighten4);
                                    }
                                    else if (avance >= 100f)
                                    {
                                        r.RelativeItem(100f).Background(Colors.Blue.Medium);
                                    }
                                    else
                                    {
                                        r.RelativeItem(avance).Background(Colors.Blue.Medium);
                                        r.RelativeItem(100f - avance).Background(Colors.Grey.Lighten4);
                                    }
                                });
                            });

                            // Barra PRESUPUESTO EJECUTADO
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Presupuesto ejecutado").Bold();

                                c.Item().Height(12).Row(r =>
                                {
                                    var financiero = (float)financieroClamped;

                                    if (financiero <= 0f)
                                    {
                                        r.RelativeItem(100f).Background(Colors.Grey.Lighten4);
                                    }
                                    else if (financiero >= 100f)
                                    {
                                        r.RelativeItem(100f).Background(Colors.Green.Medium);
                                    }
                                    else
                                    {
                                        r.RelativeItem(financiero).Background(Colors.Green.Medium);
                                        r.RelativeItem(100f - financiero).Background(Colors.Grey.Lighten4);
                                    }
                                });
                            });
                        });

                        // (Opcional) Si quieres tabla de costos, aquí sí podrías consultar CostosEjecutados
                        // para ese proyectoId, como antes.
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
