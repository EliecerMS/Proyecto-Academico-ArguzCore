using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CleanArchIdentityDemo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;
        private readonly ILogger<BlobStorageService> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            // Leer configuración
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "documentos-proyectos";

            if (string.IsNullOrEmpty(connectionString))
            {
                //throw new InvalidOperationException(
                //    "La cadena de conexión de Azure Blob Storage no está configurada. " +
                //    "Verifique la configuración 'AzureBlobStorage:ConnectionString' en User Secrets o appsettings.json");
            }

            try
            {
                // Inicializar clientes
                _blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                // Crear contenedor si no existe (solo en desarrollo)
                _containerClient.CreateIfNotExists(PublicAccessType.None);

                _logger.LogInformation("BlobStorageService inicializado correctamente. Container: {ContainerName}", _containerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar BlobStorageService");
                //throw new InvalidOperationException("No se pudo conectar a Azure Blob Storage. Verifique la cadena de conexión.", ex);
            }
        }

        // ==========================================
        // SUBIR ARCHIVO
        // ==========================================
        public async Task<string> SubirArchivoAsync(Stream archivoStream, string nombreArchivo, string contentType)
        {
            try
            {
                // Validar parámetros
                if (archivoStream == null || archivoStream.Length == 0)
                {
                    throw new ArgumentException("El stream del archivo está vacío.", nameof(archivoStream));
                }

                if (string.IsNullOrWhiteSpace(nombreArchivo))
                {
                    throw new ArgumentException("El nombre del archivo no puede estar vacío.", nameof(nombreArchivo));
                }

                // Generar nombre único para evitar sobrescrituras
                var nombreUnico = GenerarNombreUnico(nombreArchivo);

                // Obtener cliente del blob
                var blobClient = _containerClient.GetBlobClient(nombreUnico);

                // Configurar headers HTTP
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType ?? "application/octet-stream"
                };

                // Configurar metadata (opcional)
                var metadata = new Dictionary<string, string>
                {
                    { "NombreOriginal", nombreArchivo },
                    { "FechaSubida", DateTime.UtcNow.ToString("O") }
                };

                // Subir archivo
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = metadata
                };

                // Resetear posición del stream si es necesario
                if (archivoStream.CanSeek)
                {
                    archivoStream.Position = 0;
                }

                await blobClient.UploadAsync(archivoStream, uploadOptions);

                _logger.LogInformation("Archivo subido exitosamente: {NombreUnico}", nombreUnico);

                // Retornar URL completa
                return blobClient.Uri.ToString();
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error de Azure al subir archivo: {NombreArchivo}", nombreArchivo);
                throw new Exception($"Error al subir archivo a Azure Blob Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al subir archivo: {NombreArchivo}", nombreArchivo);
                throw new Exception($"Error al subir archivo: {ex.Message}", ex);
            }
        }

        // ==========================================
        // DESCARGAR ARCHIVO
        // ==========================================
        public async Task<(Stream stream, string contentType)> DescargarArchivoAsync(string blobUrl)
        {
            try
            {
                // Extraer nombre del blob desde la URL
                var nombreBlob = ExtraerNombreBlobDesdeUrl(blobUrl);

                // Obtener cliente del blob
                var blobClient = _containerClient.GetBlobClient(nombreBlob);

                // Verificar que existe
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    throw new FileNotFoundException($"El archivo no existe en Blob Storage: {nombreBlob}");
                }

                // Descargar
                var response = await blobClient.DownloadAsync();
                var contentType = response.Value.ContentType;

                // Crear MemoryStream para retornar
                var memoryStream = new MemoryStream();
                await response.Value.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _logger.LogInformation("Archivo descargado exitosamente: {NombreBlob}", nombreBlob);

                return (memoryStream, contentType);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error de Azure al descargar archivo: {BlobUrl}", blobUrl);
                throw new Exception($"Error al descargar archivo desde Azure Blob Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al descargar archivo: {BlobUrl}", blobUrl);
                throw new Exception($"Error al descargar archivo: {ex.Message}", ex);
            }
        }

        // ==========================================
        // ELIMINAR ARCHIVO
        // ==========================================
        public async Task<bool> EliminarArchivoAsync(string blobUrl)
        {
            try
            {
                // Extraer nombre del blob
                var nombreBlob = ExtraerNombreBlobDesdeUrl(blobUrl);

                // Obtener cliente del blob
                var blobClient = _containerClient.GetBlobClient(nombreBlob);

                // Eliminar si existe
                var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

                if (result.Value)
                {
                    _logger.LogInformation("Archivo eliminado exitosamente: {NombreBlob}", nombreBlob);
                }
                else
                {
                    _logger.LogWarning("Intento de eliminar archivo que no existe: {NombreBlob}", nombreBlob);
                }

                return result.Value;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error de Azure al eliminar archivo: {BlobUrl}", blobUrl);
                throw new Exception($"Error al eliminar archivo de Azure Blob Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar archivo: {BlobUrl}", blobUrl);
                throw new Exception($"Error al eliminar archivo: {ex.Message}", ex);
            }
        }

        // ==========================================
        // OBTENER URL TEMPORAL CON SAS TOKEN
        // ==========================================
        public async Task<string> ObtenerUrlTemporalAsync(string blobUrl, int duracionMinutos = 60)
        {
            try
            {
                // Extraer nombre del blob
                var nombreBlob = ExtraerNombreBlobDesdeUrl(blobUrl);

                // Obtener cliente del blob
                var blobClient = _containerClient.GetBlobClient(nombreBlob);

                // Verificar que existe
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    throw new FileNotFoundException($"El archivo no existe en Blob Storage: {nombreBlob}");
                }

                // Configurar SAS Token
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = nombreBlob,
                    Resource = "b", // "b" para blob, "c" para container
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // 5 minutos antes por sincronización de reloj
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(duracionMinutos)
                };

                // Establecer permisos (solo lectura)
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Generar SAS URI
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                _logger.LogInformation("URL temporal generada para: {NombreBlob}, válida por {Minutos} minutos", nombreBlob, duracionMinutos);

                return sasUri.ToString();
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error de Azure al generar URL temporal: {BlobUrl}", blobUrl);
                throw new Exception($"Error al generar URL temporal: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar URL temporal: {BlobUrl}", blobUrl);
                throw new Exception($"Error al generar URL temporal: {ex.Message}", ex);
            }
        }

        // ==========================================
        // VERIFICAR SI EXISTE ARCHIVO
        // ==========================================
        public async Task<bool> ExisteArchivoAsync(string blobUrl)
        {
            try
            {
                var nombreBlob = ExtraerNombreBlobDesdeUrl(blobUrl);
                var blobClient = _containerClient.GetBlobClient(nombreBlob);

                var exists = await blobClient.ExistsAsync();
                return exists.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del archivo: {BlobUrl}", blobUrl);
                return false;
            }
        }

        // ==========================================
        // OBTENER TAMAÑO DE ARCHIVO
        // ==========================================
        public async Task<long> ObtenerTamanoArchivoAsync(string blobUrl)
        {
            try
            {
                var nombreBlob = ExtraerNombreBlobDesdeUrl(blobUrl);
                var blobClient = _containerClient.GetBlobClient(nombreBlob);

                var properties = await blobClient.GetPropertiesAsync();
                return properties.Value.ContentLength;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error al obtener tamaño del archivo: {BlobUrl}", blobUrl);
                throw new Exception($"Error al obtener información del archivo: {ex.Message}", ex);
            }
        }

        // ==========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ==========================================

        /// <summary>
        /// Genera un nombre único para el archivo agregando un GUID
        /// </summary>
        private string GenerarNombreUnico(string nombreOriginal)
        {
            var extension = Path.GetExtension(nombreOriginal);
            var nombreSinExtension = Path.GetFileNameWithoutExtension(nombreOriginal);

            // Limpiar caracteres especiales
            nombreSinExtension = LimpiarNombreArchivo(nombreSinExtension);

            // Generar nombre único: guid_nombrearchivo.ext
            var nombreUnico = $"{Guid.NewGuid()}_{nombreSinExtension}{extension}";

            return nombreUnico;
        }

        /// <summary>
        /// Limpia caracteres especiales del nombre del archivo
        /// </summary>
        private string LimpiarNombreArchivo(string nombre)
        {
            // Reemplazar espacios por guiones bajos
            nombre = nombre.Replace(" ", "_");

            // Remover caracteres no permitidos
            var caracteresInvalidos = Path.GetInvalidFileNameChars();
            foreach (var c in caracteresInvalidos)
            {
                nombre = nombre.Replace(c.ToString(), "");
            }

            // Limitar longitud
            if (nombre.Length > 100)
            {
                nombre = nombre.Substring(0, 100);
            }

            return nombre;
        }

        /// <summary>
        /// Extrae el nombre del blob desde la URL completa
        /// </summary>
        private string ExtraerNombreBlobDesdeUrl(string blobUrl)
        {
            try
            {
                var uri = new Uri(blobUrl);
                var segments = uri.Segments;

                // El último segmento es el nombre del archivo
                // Ej: https://storage.blob.core.windows.net/container/folder/file.pdf
                // Último segmento: "file.pdf"
                var nombreBlob = segments[segments.Length - 1];

                // Decodificar URL (por si tiene caracteres especiales)
                nombreBlob = Uri.UnescapeDataString(nombreBlob);

                return nombreBlob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer nombre del blob desde URL: {BlobUrl}", blobUrl);
                throw new ArgumentException($"URL de blob inválida: {blobUrl}", ex);
            }
        }
    }
}
