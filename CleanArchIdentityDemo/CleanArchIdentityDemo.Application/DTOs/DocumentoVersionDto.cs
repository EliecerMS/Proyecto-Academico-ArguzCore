namespace CleanArchIdentityDemo.Application.DTOs
{
    public class DocumentoVersionDto
    {
        public int IdVersion { get; set; }
        public int DocumentoId { get; set; }
        public decimal NumeroVersion { get; set; }
        public bool EsVersionActual { get; set; }

        // Archivo
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string NombreBlob { get; set; } = string.Empty;
        public string RutaBlobCompleta { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }

        // Auditoría
        public string SubidoPor { get; set; } = string.Empty;
        public string NombreSubidoPor { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public string? Comentarios { get; set; }

        // Del documento padre
        public string NombreDocumento { get; set; } = string.Empty;
        public string CategoriaDocumento { get; set; } = string.Empty;

        // Propiedades calculadas
        public string TamanoFormateado => FormatearTamano(TamanoBytes);
        public string IconoArchivo => ObtenerIcono(TipoArchivo);

        private static string FormatearTamano(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string ObtenerIcono(string tipoArchivo)
        {
            return tipoArchivo.ToLower() switch
            {
                "application/pdf" => "fa-file-pdf text-danger",
                var t when t.Contains("word") => "fa-file-word text-primary",
                var t when t.Contains("excel") || t.Contains("spreadsheet") => "fa-file-excel text-success",
                var t when t.Contains("image") => "fa-file-image text-info",
                var t when t.Contains("zip") || t.Contains("compressed") => "fa-file-archive text-warning",
                "application/acad" or "image/vnd.dwg" => "fa-drafting-compass text-warning",
                _ => "fa-file text-secondary"
            };
        }
    }
}
