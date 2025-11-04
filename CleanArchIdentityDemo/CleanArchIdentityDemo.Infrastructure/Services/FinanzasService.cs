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
               NombreProyecto = p.Proyecto.Nombre,
               RutaComprobante = p.RutaComprobante
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
        //Método para proyecto 
        public async Task<IEnumerable<ProyectoDto>> ListarProyectosAsync()
        {
            var proyectos = await _context.Proyectos
                .Include(p => p.EstadoProyecto)
                .ToListAsync();

            return proyectos.Select(p => new ProyectoDto
            {
                IdProyecto = p.IdProyecto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CodigoProyecto = p.CodigoProyecto,
                FechaFinalPropuesta = p.FechaFinalPropuesta,
                IdEstadoProyecto = p.EstadoProyectoId,
                Presupuesto = p.Presupuesto,
                EstadoProyecto = p.EstadoProyecto.NombreEstado
            });
        }

        //Método para registrar pago
        public async Task<PagoProveedorDto> RegistrarPagoProveedorAsync(PagoProveedorDto dto)
        {
            // 1️ Crear el nuevo registro del pago
            var nuevoPago = new PagoProveedor
            {
                ProyectoId = dto.ProyectoId,
                ProveedorId = dto.ProveedorId,
                Monto = dto.Monto,
                FechaPago = dto.FechaPago,
                Descripcion = dto.Descripcion,
                RutaComprobante = dto.RutaComprobante
            };

            _context.PagosProveedores.Add(nuevoPago);
            await _context.SaveChangesAsync();

            var pago = await _context.PagosProveedores
                .Include(p => p.Proveedor)
                .Include(p => p.Proyecto)
                .FirstOrDefaultAsync(p => p.IdPago == nuevoPago.IdPago);

            var resultado = new PagoProveedorDto
            {
                IdPago = pago.IdPago,
                ProyectoId = pago.ProyectoId,
                ProveedorId = pago.ProveedorId,
                NombreProveedor = pago.Proveedor.NombreProveedor,
                ContactoProveedor = pago.Proveedor.Contacto,
                NombreProyecto = pago.Proyecto.Nombre,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                Descripcion = pago.Descripcion,
                RutaComprobante = pago.RutaComprobante
            };

            return resultado;
        }
        //Métodos de costos ejecutados 
        public async Task<IEnumerable<CostoEjecutadoDto>> ListarCostosEjecutadosAsync()
        {
            return await _context.CostosEjecutados
                .Include(c => c.Proyecto)
                .Select(c => new CostoEjecutadoDto
                {
                    IdCosto = c.IdCosto,
                    ProyectoId = c.ProyectoId,
                    NombreProyecto = c.Proyecto.Nombre,
                    CategoriaGasto = c.CategoriaGasto,
                    Monto = c.Monto,
                    Fecha = c.Fecha,
                    Descripcion = c.Descripcion,
                    RutaComprobante = c.RutaComprobante
                })
                .ToListAsync();
        }

        public async Task<CostoEjecutadoDto?> ObtenerCostoEjecutadoPorIdAsync(int id)
        {
            var c = await _context.CostosEjecutados
                .Include(p => p.Proyecto)
                .FirstOrDefaultAsync(p => p.IdCosto == id);

            if (c == null) return null;

            return new CostoEjecutadoDto
            {
                IdCosto = c.IdCosto,
                ProyectoId = c.ProyectoId,
                NombreProyecto = c.Proyecto?.Nombre ?? string.Empty,
                CategoriaGasto = c.CategoriaGasto,
                Monto = c.Monto,
                Fecha = c.Fecha,
                Descripcion = c.Descripcion,
                RutaComprobante = c.RutaComprobante
            };
        }

        public async Task<bool> CrearCostoEjecutadoAsync(CostoEjecutadoDto dto)
        {
            try
            {
                var entity = new CostoEjecutado
                {
                    ProyectoId = dto.ProyectoId,
                    CategoriaGasto = dto.CategoriaGasto,
                    Monto = dto.Monto,
                    Fecha = dto.Fecha,
                    Descripcion = dto.Descripcion,
                    RutaComprobante = dto.RutaComprobante
                };

                _context.CostosEjecutados.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<IEnumerable<CostoEjecutadoDto>> ListarCostosPorProyectoAsync(int proyectoId)
        {
            var costos = await _context.CostosEjecutados
                .Where(c => c.ProyectoId == proyectoId)
                .Include(c => c.Proyecto)
                .ToListAsync();

            return costos.Select(c => new CostoEjecutadoDto
            {
                IdCosto = c.IdCosto,
                ProyectoId = c.ProyectoId,
                NombreProyecto = c.Proyecto.Nombre,
                CategoriaGasto = c.CategoriaGasto,
                Monto = c.Monto,
                Fecha = c.Fecha,
                Descripcion = c.Descripcion,
                RutaComprobante = c.RutaComprobante
            });
        }

        public async Task<bool> EditarCostoEjecutadoAsync(CostoEjecutadoDto dto)
        {
            try
            {
                var entity = await _context.CostosEjecutados.FindAsync(dto.IdCosto);
                if (entity == null)
                    return false;

                // Actualizar campos editables
                entity.ProyectoId = dto.ProyectoId;
                entity.CategoriaGasto = dto.CategoriaGasto;
                entity.Monto = dto.Monto;
                entity.Fecha = dto.Fecha;
                entity.Descripcion = dto.Descripcion;

                // Si se subió un nuevo comprobante, reemplazarlo
                if (!string.IsNullOrEmpty(dto.RutaComprobante))
                {
                    entity.RutaComprobante = dto.RutaComprobante;
                }

                _context.CostosEjecutados.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar costo ejecutado: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> EliminarCostoEjecutadoAsync(int id)
        {
            try
            {
                var entity = await _context.CostosEjecutados.FindAsync(id);
                if (entity == null)
                    return false;

                _context.CostosEjecutados.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar costo ejecutado: {ex.Message}");
                return false;
            }
        }
    }
}
