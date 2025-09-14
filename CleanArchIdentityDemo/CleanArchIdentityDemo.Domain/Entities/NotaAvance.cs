using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class NotaAvance
    {
        // PK
        [Key]
        public int IdNota { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public string Descripcion { get; set; } = string.Empty;
        public bool Destacada { get; set; }
        public DateTime FechaNota { get; set; }

        // FK -> Usuarios.IdUsuario (quien creó la nota)
        public string CreadoPor { get; set; }

        // Navegaciones
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
