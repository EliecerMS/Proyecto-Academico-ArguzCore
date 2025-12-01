using System;
using System.Threading.Tasks;
using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class AnaliticaService : IAnaliticaService
    {
        public Task<byte[]> GenerarPdfComparacionAsync(ComparacionAnaliticaDto c)
        {
            // Asegurar licencia de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);

                    // Encabezado
                    page.Header()
                        .Text("Comparativa de Proyectos")
                        .FontSize(20)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    // Contenido principal
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // Subtítulo con mes
                        col.Item().Text(txt =>
                        {
                            txt.DefaultTextStyle(x => x.FontSize(11));

                            txt.Span("Periodo analizado: ").SemiBold();

                            var mesA = c.MesA.HasValue ? $"Mes {c.MesA}" : "Todos los meses / Actual";
                            var mesB = c.MesB.HasValue ? $"Mes {c.MesB}" : "Todos los meses / Actual";

                            txt.Span($"Proyecto A: {mesA} | Proyecto B: {mesB}");
                        });


                        // Tabla de comparación
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Concepto
                                columns.RelativeColumn();  // Proyecto A
                                columns.RelativeColumn();  // Proyecto B
                            });

                            // Encabezados
                            table.Header(h =>
                            {
                                h.Cell().Text("");
                                h.Cell().AlignCenter().Text(c.NombreProyectoA).SemiBold();
                                h.Cell().AlignCenter().Text(c.NombreProyectoB).SemiBold();
                            });

                            void Row(string label, string a, string b)
                            {
                                table.Cell().Text(label);
                                table.Cell().AlignCenter().Text(a);
                                table.Cell().AlignCenter().Text(b);
                            }

                            Row("Estado",
                                c.EstadoProyectoA,
                                c.EstadoProyectoB);

                            Row("Avance físico (%)",
                                $"{c.AvanceFisicoA} %",
                                $"{c.AvanceFisicoB} %");

                            Row("Presupuesto ejecutado (%)",
                                $"{c.PresupuestoEjecutadoPorcentajeA} %",
                                $"{c.PresupuestoEjecutadoPorcentajeB} %");

                            Row("Desviación (%)",
                                $"{c.DesviacionPorcentajeA} %",
                                $"{c.DesviacionPorcentajeB} %");

                            Row("Nivel de riesgo",
                                c.RiesgoA,
                                c.RiesgoB);
                        });

                        // Notas al pie
                        col.Item().Text("Nota: La desviación se calcula como Valor Ganado - Costo Ejecutado, expresado como porcentaje del presupuesto planificado.")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    // Footer
                    page.Footer().AlignRight().Text(txt =>
                    {
                        txt.Span("Generado el ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                });
            });

            var pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }
    }
}
