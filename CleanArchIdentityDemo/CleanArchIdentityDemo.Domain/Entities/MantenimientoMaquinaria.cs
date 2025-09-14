using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class MantenimientoMaquinaria
    {
        // PK
        [Key]
        public int IdMantenimiento { get; set; }

        // FK -> Maquinarias.IdMaquinaria
        public int MaquinariaId { get; set; }

        // Atributos
        public DateTime FechaProgramada { get; set; }
        public DateTime FechaCompletado { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Navegación
        [ForeignKey(nameof(MaquinariaId))]
        public Maquinaria Maquinaria { get; set; } = null!;
    }
}
