using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class PagoProveedor
    {
        // PK
        [Key]
        public int IdPago { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // FK -> Proveedores.IdProveedor
        public int ProveedorId { get; set; }

        // Atributos
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string RutaComprobante { get; set; } = string.Empty;

        // Navegaciones
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;

        [ForeignKey(nameof(ProveedorId))]
        public Proveedor Proveedor { get; set; } = null!;
    }
}
