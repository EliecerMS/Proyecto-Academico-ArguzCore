using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Incidente
    {
        // PK
        [Key]
        public int IdIncidente { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }

        // FK -> Usuarios.IdUsuario (quien creó el incidente)
        public string CreadoPor { get; set; }

        // Navegaciones
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
