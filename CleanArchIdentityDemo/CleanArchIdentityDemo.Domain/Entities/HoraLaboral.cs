using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class HoraLaboral
    {
        // PK
        [Key]
        public int IdHoraLaboral { get; set; }

        // FK -> PersonalProyecto.IdPersonalProyecto
        public int PersonalProyectoId { get; set; }

        // Atributos
        public DateTime HoraEntrada { get; set; }
        public DateTime HoraSalida { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Navegación padre
        [ForeignKey(nameof(PersonalProyectoId))]
        public PersonalProyecto PersonalProyecto { get; set; } = null!;
    }
}
