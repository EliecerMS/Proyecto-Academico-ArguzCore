namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IBlobStorageService
    {
        // Sube un archivo al Blob Storage
        Task<string> SubirArchivoAsync(Stream archivoStream, string nombreArchivo, string contentType);
        // Descarga un archivo desde Blob Storage
        Task<(Stream stream, string contentType)> DescargarArchivoAsync(string blobUrl);
        // Elimina un archivo del Blob Storage
        Task<bool> EliminarArchivoAsync(string blobUrl);
        // Genera una URL temporal con SAS Token para acceso al archivo
        Task<string> ObtenerUrlTemporalAsync(string blobUrl, int duracionMinutos = 60);
        // Verifica si un archivo existe en el Blob Storage
        Task<bool> ExisteArchivoAsync(string blobUrl);
        // Obtiene el tamaño de un archivo en bytes
        Task<long> ObtenerTamanoArchivoAsync(string blobUrl);
    }
}
