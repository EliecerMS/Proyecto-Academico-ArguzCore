using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class DocumentoVersion
    {
        [Key]
        public int IdVersion { get; set; }


        public int DocumentoId { get; set; }


        [Column(TypeName = "decimal(3,1)")]
        public decimal NumeroVersion { get; set; }

        public bool EsVersionActual { get; set; } = false;

        // INFORMACIÓN DEL ARCHIVO EN BLOB STORAGE
        public string NombreArchivoOriginal { get; set; } = string.Empty;

        public string NombreBlob { get; set; } = string.Empty;

        public string RutaBlobCompleta { get; set; } = string.Empty;

        public string TipoArchivo { get; set; } = string.Empty;


        public long TamanoBytes { get; set; }

        // AUDITORÍA

        public string SubidoPor { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        public string? Comentarios { get; set; }

        public bool Activo { get; set; } = true;



        [ForeignKey(nameof(DocumentoId))]
        public Documento Documento { get; set; } = null!;
    }
}
