using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class MaquinariaProyecto
    {
        // PK
        [Key]
        public int IdMaquinariaProyecto { get; set; }

        // FK -> Maquinarias.IdMaquinaria
        public int MaquinariaId { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public DateTime FechaAsignacion { get; set; }

        // Navegaciones
        [ForeignKey(nameof(MaquinariaId))]
        public Maquinaria Maquinaria { get; set; } = null!;

        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
