using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class DevolucionMaterial
    {
        // PK
        [Key]
        public int IdDevolucion { get; set; }

        // FK -> MaterialesProyecto.IdMaterialProyecto
        public int MaterialProyectoId { get; set; }

        // Atributos
        public int CantidadDevuelta { get; set; }
        public DateTime FechaDevolucion { get; set; }

        // Navegación
        [ForeignKey(nameof(MaterialProyectoId))]
        public MaterialProyecto MaterialProyecto { get; set; } = null!;
    }
}
