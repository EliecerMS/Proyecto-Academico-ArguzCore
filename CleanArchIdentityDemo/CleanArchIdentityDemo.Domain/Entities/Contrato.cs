using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Contrato
    {
        // PK
        [Key]
        public int IdContrato { get; set; }

        // Atributos
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string RutaDocumento { get; set; } = string.Empty;

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Navegación
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
