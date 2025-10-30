namespace CleanArchIdentityDemo.Application.DTOs
{
    public class CrearDocumentoSimpleDto
    {
        public int ProyectoId { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string? CategoriaDocumento { get; set; }
        public string? Descripcion { get; set; }

        // Archivo único (sin versiones)
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string RutaBlobCompleta { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }

        public string CreadoPor { get; set; } = string.Empty;
    }
}
