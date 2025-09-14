using System.ComponentModel.DataAnnotations;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class EstadoProyecto
    {
        // PK
        [Key]
        public int IdEstadoProyecto { get; set; }

        // Atributos
        public string NombreEstado { get; set; } = string.Empty;

        // Colecciones (1 Estado -> N Proyectos)
        public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
    }
}
