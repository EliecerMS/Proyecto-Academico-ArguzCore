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

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerarInformeProyectoAsync(int proyectoId)
        {
            var proyecto = await _context.Proyectos
                .Include(p => p.Tareas)
                .FirstOrDefaultAsync(p => p.IdProyecto == proyectoId);

            if (proyecto == null)
                throw new Exception("Proyecto no encontrado.");

            // Avance físico (promedio de las tareas) 
            int avanceFisico = 0;
            if (proyecto.Tareas != null && proyecto.Tareas.Any())
            {
                avanceFisico = (int)Math.Round(
                    proyecto.Tareas.Average(t => t.PorcentajeAvance)
                );
            }

            // Costos ejecutados
            var costos = await _context.CostosEjecutados
                .Where(c => c.ProyectoId == proyectoId)
                .ToListAsync();

            decimal totalEjecutado = costos.Sum(c => c.Monto);
            decimal porcentajeFinanciero = 0;

            if (proyecto.Presupuesto > 0)
                porcentajeFinanciero = Math.Round((totalEjecutado / proyecto.Presupuesto) * 100, 2);

            decimal desviacion = proyecto.Presupuesto - totalEjecutado;

            // Limitar 0–100 para las barras
            int avanceClamped = Math.Clamp(avanceFisico, 0, 100);
            int financieroClamped = (int)Math.Clamp(porcentajeFinanciero, 0, 100);

            //Documento QuestPDF 
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
                            //Barra de AVANCE FÍSICO
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Avance físico").Bold();

                                c.Item().Height(12).Row(r =>
                                {
                                    var avance = (float)avanceClamped;

                                    if (avance <= 0f)
                                    {
                                        // 0% -> solo barra gris
                                        r.RelativeItem(1f).Background(Colors.Grey.Lighten4);
                                    }
                                    else if (avance >= 100f)
                                    {
                                        // 100% -> solo barra azul
                                        r.RelativeItem(1f).Background(Colors.Blue.Medium);
                                    }
                                    else
                                    {
                                        // entre 0 y 100 -> tramo azul + tramo gris
                                        r.RelativeItem(avance).Background(Colors.Blue.Medium);
                                        r.RelativeItem(100f - avance).Background(Colors.Grey.Lighten4);
                                    }
                                });
                            });

                            //Barra de PRESUPUESTO EJECUTADO
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Presupuesto ejecutado").Bold();

                                c.Item().Height(12).Row(r =>
                                {
                                    var financiero = (float)financieroClamped;

                                    if (financiero <= 0f)
                                    {
                                        // 0% -> solo barra gris
                                        r.RelativeItem(1f).Background(Colors.Grey.Lighten4);
                                    }
                                    else if (financiero >= 100f)
                                    {
                                        // 100% -> solo barra verde
                                        r.RelativeItem(1f).Background(Colors.Green.Medium);
                                    }
                                    else
                                    {
                                        // entre 0 y 100 -> tramo verde + tramo gris
                                        r.RelativeItem(financiero).Background(Colors.Green.Medium);
                                        r.RelativeItem(100f - financiero).Background(Colors.Grey.Lighten4);
                                    }
                                });
                            });
                        });


                        //Tabla de costos 
                        if (costos.Any())
                        {
                            col.Item().Text("Detalle de costos ejecutados")
                                .FontSize(14).Bold();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Categoría
                                    columns.RelativeColumn(2); // Fecha
                                    columns.RelativeColumn(2); // Monto
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Categoría").Bold();
                                    header.Cell().Text("Fecha").Bold();
                                    header.Cell().Text("Monto").Bold();
                                });

                                foreach (var c in costos)
                                {
                                    table.Cell().Text(c.CategoriaGasto);
                                    table.Cell().Text(c.Fecha.ToString("dd/MM/yyyy"));
                                    table.Cell().Text($"₡{c.Monto:N0}");
                                }
                            });
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}