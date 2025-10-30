namespace CleanArchIdentityDemo.Application.DTOs
{
    public class AgregarVersionDto
    {
        public int DocumentoId { get; set; }  // Documento existente seleccionado
        public decimal? NumeroVersionManual { get; set; }  // Opcional: permitir especificar versión

        // Archivo de la nueva versión
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string RutaBlobCompleta { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }

        public string SubidoPor { get; set; } = string.Empty;
        public string? Comentarios { get; set; }
    }
}
