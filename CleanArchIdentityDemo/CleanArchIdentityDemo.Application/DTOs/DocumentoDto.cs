namespace CleanArchIdentityDemo.Application.DTOs
{
    public class DocumentoDto
    {
        public int IdDocumento { get; set; }

        public int ProyectoId { get; set; }

        // Atributos
        public string NombreDocumento { get; set; } = string.Empty;
        public string CategoriaDocumento { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // TIPO DE DOCUMENTO
        public string TipoDocumento { get; set; } = string.Empty;

        // CONTROL DE VERSIONES (Solo para Versionado)

        public decimal VersionActual { get; set; }
        public int TotalVersiones { get; set; }


        public string? NombreArchivoOriginal { get; set; }  // Solo para tipo "Simple"
        public string? RutaBlobCompleta { get; set; }
        public string? TipoArchivo { get; set; }
        public long? TamanoBytes { get; set; }

        //campos para auditoria
        public string SubidoPor { get; set; } = string.Empty;
        public string NombreCreadoPor { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public DateTime UltimaModificacion { get; set; }

        // Versiones (solo para versionados)
        public List<DocumentoVersionDto> Versiones { get; set; } = new();

        // Propiedades calculadas
        public string TamanoFormateado
        {
            get
            {
                if (!TamanoBytes.HasValue) return "-";
                return FormatearTamano(TamanoBytes.Value);
            }
        }

        public string IconoArchivo
        {
            get
            {
                if (string.IsNullOrEmpty(TipoArchivo)) return "fa-file text-secondary";
                return ObtenerIcono(TipoArchivo);
            }
        }

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
