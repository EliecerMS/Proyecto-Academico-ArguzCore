using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Tarea
    {
        // PK
        [Key]
        public int IdTarea { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public string NombreTarea { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaInicioEsperada { get; set; }
        public DateTime FechaFinalEsperada { get; set; }
        public int PorcentajeAvance { get; set; }


        // Navegación (padre)
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
