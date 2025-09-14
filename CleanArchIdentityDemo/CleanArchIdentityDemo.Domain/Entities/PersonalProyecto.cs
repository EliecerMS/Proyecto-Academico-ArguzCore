using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class PersonalProyecto
    {
        // PK
        [Key]
        public int IdPersonalProyecto { get; set; }

        // FK -> Usuarios.IdUsuario
        public string UsuarioId { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public DateTime FechaAsignacion { get; set; }

        // Navegaciones (padres)
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;

        // Colecciones
        public ICollection<HoraLaboral> HorasLaborales { get; set; } = new List<HoraLaboral>();
    }
}
