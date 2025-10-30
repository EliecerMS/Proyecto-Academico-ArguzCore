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
        public string? Descripcion { get; set; }

        // TIPO DE DOCUMENTO
        public string TipoDocumento { get; set; } = "Versionado";

        // CONTROL DE VERSIONES (Solo para Versionado)

        [Column(TypeName = "decimal(3,1)")]
        public decimal VersionActual { get; set; } = 1.0m;
        public int TotalVersiones { get; set; } = 1;


        public string? NombreArchivoOriginal { get; set; }  // Solo para tipo "Simple"

        public string? NombreBlob { get; set; }
        public string? RutaBlobCompleta { get; set; }
        public string? TipoArchivo { get; set; }
        public long? TamanoBytes { get; set; }

        //campos para auditoria
        // FK -> Usuarios.IdUsuario (quien subió)
        public string SubidoPor { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; } = DateTime.Now;
        public DateTime UltimaModificacion { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;


        // Navegaciones (padre)
        [ForeignKey(nameof(ProyectoId))]
        public Proyecto Proyecto { get; set; } = null!;


        // Solo para documentos versionados
        public ICollection<DocumentoVersion> Versiones { get; set; } = new List<DocumentoVersion>();
    }
}
