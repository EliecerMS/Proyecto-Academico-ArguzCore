using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class FinanzasService : IFinanzasService
    {
        private readonly ApplicationDbContext _context;
        public FinanzasService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PagoProveedorDto> DatosPagoProveedorPorIdAsync(int IdPago)
        {
            var DatosPago = await _context.PagosProveedores
            .Where(p => p.IdPago == IdPago)
            .Select(p => new PagoProveedorDto
            {
                IdPago = p.IdPago,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                Descripcion = p.Descripcion,
                NombreProveedor = p.Proveedor.NombreProveedor,
                ProveedorId = p.ProveedorId,
                NombreProyecto = p.Proyecto.Nombre,
                ProyectoId = p.Proyecto.IdProyecto,
                ContactoProveedor = p.Proveedor.Contacto
            })
            .FirstOrDefaultAsync();

            if (DatosPago != null)
            {

                return DatosPago;

            }
            else
            {
                return null;
            }
        }

        //implementacion de los metodos para realizar operaciones en la vista de Finanzas
        public byte[] GenerarComprobante(int IdPago)
        {
            // Await del metodo asincrono PagoProveedorDto que trae los datos del pago por su Id
            PagoProveedorDto datos = DatosPagoProveedorPorIdAsync(IdPago).GetAwaiter().GetResult();
            if (datos != null)
            {

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // Header
                        page.Header()
                            .Column(column =>
                            {
                                column.Item().Text("Generación de comprobante")
                                    .FontSize(20)
                                    .Bold()
                                    .AlignCenter();

                                column.Item().PaddingVertical(10);

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"Recibo No: {datos.IdPago}").Bold();
                                    row.RelativeItem().Text($"Fecha de registro de pago: {datos.FechaPago:dd/MM/yyyy}").AlignRight();
                                });
                            });

                        // Content
                        page.Content()
                            .PaddingVertical(20)
                            .Column(column =>
                            {
                                // Información del proveedor
                                column.Item().Element(ComposeProveedor);

                                column.Item().PaddingVertical(10);

                                // Información del proyecto
                                column.Item().Element(ComposeProyecto);

                                column.Item().PaddingVertical(10);

                                // Detalles del pago
                                column.Item().Element(ComposeDetalles);

                                column.Item().PaddingVertical(20);

                                // Total
                                column.Item().Element(ComposeTotal);
                            });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(txt =>
                            {
                                //txt.Line("___________________________");
                                txt.Span("Arguz Inversiones").FontSize(10);
                            });

                        // Métodos helper
                        void ComposeProveedor(IContainer container)
                        {
                            container.Background(Colors.Grey.Lighten3)
                                .Padding(10)
                                .Column(column =>
                                {
                                    column.Item().Text("PROVEEDOR").Bold().FontSize(12);
                                    column.Item().Text($"Nombre: {datos.NombreProveedor}");
                                    column.Item().Text($"Contacto: {datos.ContactoProveedor}");
                                });
                        }

                        void ComposeProyecto(IContainer container)
                        {
                            container.Background(Colors.Grey.Lighten4)
                                .Padding(10)
                                .Column(column =>
                                {
                                    column.Item().Text("PROYECTO").Bold().FontSize(12);
                                    column.Item().Text($"Nombre: {datos.NombreProyecto}");
                                });
                        }

                        void ComposeDetalles(IContainer container)
                        {
                            container.Column(column =>
                            {
                                column.Item().Text("DETALLES DEL PAGO").Bold().FontSize(12);

                                column.Item().PaddingVertical(5);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Medium)
                                            .Padding(5).Text("Concepto").Bold();
                                        header.Cell().Background(Colors.Grey.Medium)
                                            .Padding(5).Text("Monto").Bold().AlignRight();
                                    });

                                    // Row
                                    table.Cell().Border(1).Padding(5)
                                        .Text(datos.Descripcion);
                                    table.Cell().Border(1).Padding(5)
                                        .Text($"₡{datos.Monto:N2}").AlignRight();
                                });
                            });
                        }

                        void ComposeTotal(IContainer container)
                        {
                            container.AlignRight()
                                .Background(Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text($"TOTAL: ₡{datos.Monto:N2}")
                                .FontSize(16)
                                .Bold();
                        }
                    });
                });
                return document.GeneratePdf();
            }
            else
            {
                return null;
            }

        }

        public async Task<IEnumerable<PagoProveedorDto>> ListarPagosProveedoresAsync()
        {
            var pagosProveedores = await _context.PagosProveedores
           .Include(p => p.Proveedor)     // Carga relaciones en una sola consulta para navegar y obtener datos de las tablas relacionadas como lo son Proveedor y Proyecto en este caso
           .Include(p => p.Proyecto)
           .Select(p => new PagoProveedorDto  // Crear una instancia del Dto para mapear o pasarle el resultado al Dto
           {
               IdPago = p.IdPago,
               Monto = p.Monto,
               FechaPago = p.FechaPago,
               Descripcion = p.Descripcion,
               ProveedorId = p.ProveedorId,
               NombreProveedor = p.Proveedor.NombreProveedor,
               ProyectoId = p.ProyectoId,
               NombreProyecto = p.Proyecto.Nombre
           })
           .ToListAsync();  // es Async realmente y es List

            return pagosProveedores;//retorna la lista de pagos a proveedores
        }

        public async Task<IEnumerable<ProveedorDto>> ListarProveedoresAsync()
        {
            var Proveedores = await _context.Proveedores
           .Select(p => new ProveedorDto  // Mapea todos los registros de la tabla Proveedores al Dto
           {
               IdProveedor = p.IdProveedor,
               NombreProveedor = p.NombreProveedor,
               Contacto = p.Contacto
           })
           .ToListAsync();  // es Async realmente y list

            return Proveedores;//retorna la lista de proveedores
        }
        public async Task<bool> CrearProveedorAsync(ProveedorDto DatosProveedor)
        {
            bool ProveedorCreado = false;

            if (DatosProveedor != null)
            {
                _context.Proveedores.Add(new Proveedor
                {
                    NombreProveedor = DatosProveedor.NombreProveedor,
                    Contacto = DatosProveedor.Contacto
                });

                ProveedorCreado = await _context.SaveChangesAsync() > 0;
            }

            return ProveedorCreado;
        }

        public async Task<bool> EditarProveedorAsync(ProveedorDto DatosProveedor)
        {
            bool ProveedorEditado = false;
            if (DatosProveedor != null)
            {
                var proveedor = await _context.Proveedores.FindAsync(DatosProveedor.IdProveedor);
                if (proveedor != null)
                {
                    proveedor.NombreProveedor = DatosProveedor.NombreProveedor;
                    proveedor.Contacto = DatosProveedor.Contacto;
                    ProveedorEditado = await _context.SaveChangesAsync() > 0;
                }
            }
            return ProveedorEditado;
        }

        public async Task<bool> EliminarProveedorAsync(int IdProveedor)
        {
            bool ProveedorEliminado = false;
            var proveedor = await _context.Proveedores.FindAsync(IdProveedor);
            if (proveedor != null)
            {
                _context.Proveedores.Remove(proveedor);
                ProveedorEliminado = await _context.SaveChangesAsync() > 0;
            }
            return ProveedorEliminado;
        }
    }
}
