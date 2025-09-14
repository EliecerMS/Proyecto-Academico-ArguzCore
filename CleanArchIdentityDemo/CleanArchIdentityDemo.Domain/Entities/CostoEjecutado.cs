using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class CostoEjecutado
    {
        // PK
        [Key]
        public int IdCosto { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public string CategoriaGasto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string RutaComprobante { get; set; } = string.Empty;

        // Navegación
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
