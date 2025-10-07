using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class SolicitudMaterial
    {
        // PK
        [Key]
        public int IdSolicitud { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public DateTime FechaSolicitud { get; set; }
        public string EstadoSolicitud { get; set; } = string.Empty;
        public string ObservacionesBodeguero { get; set; } = string.Empty;


        // Navegación
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;


        // Colección
        public List<MaterialSolicitado> MaterialesSolicitados { get; set; } = new List<MaterialSolicitado>();
    }
}
