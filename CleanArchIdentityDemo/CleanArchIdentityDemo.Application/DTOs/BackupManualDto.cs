namespace CleanArchIdentityDemo.Application.DTOs
{
    public class BackupManualDto
    {
        public string NombreArchivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public long TamanoBytes { get; set; }
        public string UrlDescarga { get; set; }

        public string TamanoFormateado
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = TamanoBytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}
