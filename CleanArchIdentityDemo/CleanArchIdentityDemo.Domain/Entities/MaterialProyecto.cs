using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class MaterialProyecto
    {
        // PK
        [Key]
        public int IdMaterialProyecto { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // FK -> Materiales.IdMaterial
        public int MaterialId { get; set; }

        // Atributos
        public int CantidadEnObra { get; set; }

        // Navegaciones
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;

        // Colecciones
        public ICollection<DevolucionMaterial> Devoluciones { get; set; } = new List<DevolucionMaterial>();
    }
}
