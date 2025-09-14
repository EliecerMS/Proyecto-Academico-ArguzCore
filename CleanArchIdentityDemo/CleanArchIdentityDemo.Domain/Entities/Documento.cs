using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Documento
    {
        // PK
        [Key]
        public int IdDocumento { get; set; }

        // FK -> Proyectos.IdProyecto
        public int ProyectoId { get; set; }

        // Atributos
        public string NombreDocumento { get; set; } = string.Empty;
        public string CategoriaDocumento { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime FechaSubida { get; set; }

        // FK -> Usuarios.IdUsuario (quien subió)
        public string SubidoPor { get; set; }

        public bool EsVersionGeneral { get; set; }

        // Navegaciones (padre)
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;
    }
}
