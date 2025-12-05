using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class BDRespaldoService : IBDRespladosService
    {
        private readonly string _subscriptionId;
        private readonly string _resourceGroup;
        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly string _blobStorageConnectionString;
        private readonly string _containerName;
        private readonly string _sqlUser;
        private readonly string _sqlPassword;
        private readonly string _storageAccountName;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuditoriaService> _logger;

        public BDRespaldoService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuditoriaService> logger)
        {
            _logger = logger;

            // AZURE CONFIGURATION (lee de múltiples fuentes)

            _subscriptionId = configuration["Azure:SubscriptionId"]              // User Secrets local
                                                                                 //?? configuration["Azure__SubscriptionId"]                        // Azure App Service
                ?? throw new InvalidOperationException("Azure:SubscriptionId no configurado");

            _resourceGroup = configuration["Azure:ResourceGroup"]
                //?? configuration["Azure__ResourceGroup"]
                ?? throw new InvalidOperationException("Azure:ResourceGroup no configurado");

            _serverName = configuration["Azure:ServerName"]
                //?? configuration["Azure__ServerName"]
                ?? throw new InvalidOperationException("Azure:ServerName no configurado");

            _databaseName = configuration["Azure:DatabaseName"]
                //?? configuration["Azure__DatabaseName"]
                ?? throw new InvalidOperationException("Azure:DatabaseName no configurado");

            _sqlUser = configuration["Azure:SqlUser"]
                //?? configuration["Azure__SqlUser"]
                ?? throw new InvalidOperationException("Azure:SqlUser no configurado");

            _sqlPassword = configuration["Azure:SqlPassword"]
                //?? configuration["Azure__SqlPassword"]
                ?? throw new InvalidOperationException("Azure:SqlPassword no configurado");

            // STORAGE CONFIGURATION

            _storageAccountName = configuration["AzureBackupStorage:StorageAccount"]  // User Secrets local
                ?? configuration["Azure:BackupStorageAccount"]                        // Alternativo
                                                                                      //?? configuration["Azure__BackupStorageAccount"]                       // Azure App Service
                ?? throw new InvalidOperationException("AzureBackupStorage:StorageAccount no configurado");

            _containerName = configuration["AzureBackupStorage:ContainerName"]        // User Secrets local
                ?? configuration["Azure:BackupContainerName"]                         // Alternativo
                                                                                      //?? configuration["Azure__BackupContainerName"]                        // Azure App Service
                ?? "backups";                                                         // Fallback

            // BLOB STORAGE CONNECTION STRING

            _blobStorageConnectionString = configuration.GetConnectionString("AzureBlobStorageConnectionString")  // Azure
                ?? configuration["AzureBlobStorage:ConnectionString"]                                            // Local
                ?? throw new InvalidOperationException("AzureBlobStorage ConnectionString no configurado");

            _logger.LogInformation("BDRespaldoService inicializado correctamente");
        }

        // LISTAR PUNTOS DE RESTAURACIÓN DISPONIBLES (PITR)
        public async Task<List<PuntoRestauracionDto>> ListarPuntosRestauracionAsync()
        {
            try
            {
                //instanciar cliente ARM
                var credential = new DefaultAzureCredential();
                var armClient = new ArmClient(credential);
                var databaseResourceId = SqlDatabaseResource.CreateResourceIdentifier(_subscriptionId, _resourceGroup, _serverName, _databaseName);
                var databaseResource = armClient.GetSqlDatabaseResource(databaseResourceId);

                // Obtener los puntos de restauración
                var restorePoints = databaseResource.GetSqlServerDatabaseRestorePoints();

                var puntos = new List<PuntoRestauracionDto>();

                // Usar await foreach para iterar y mapear manualmente
                await foreach (var restorePoint in restorePoints.GetAllAsync())
                {
                    puntos.Add(new PuntoRestauracionDto
                    {
                        NombrePunto = restorePoint.Data.Name,
                        FechaCreacion = restorePoint.Data.RestorePointCreatedOn,
                        TipoPunto = restorePoint.Data.RestorePointType.ToString(),
                        FechaMasAntigua = restorePoint.Data.EarliestRestoreOn ?? DateTimeOffset.MinValue
                    });
                }

                // Ordenar la lista después de iterar
                return puntos.OrderByDescending(p => p.FechaCreacion).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar puntos de restauración");
                return new List<PuntoRestauracionDto>();
            }
        }


        // OBTENER FECHA DEL BACKUP MÁS ANTIGUO

        public async Task<string> ObtenerFechaBackupMasAntiguoAsync()
        {
            try
            {
                var credential = new DefaultAzureCredential();
                var armClient = new ArmClient(credential);

                var subscriptionResource = armClient.GetSubscriptionResource(
                    new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));

                var resourceGroupResource = await subscriptionResource
                    .GetResourceGroups()
                    .GetAsync(_resourceGroup);

                var sqlServerResource = await resourceGroupResource.Value
                    .GetSqlServers()
                    .GetAsync(_serverName);

                var databaseResource = await sqlServerResource.Value
                    .GetSqlDatabases()
                    .GetAsync(_databaseName);

                // Fecha más antigua de restauración (earliest restore point)
                var earliestRestore = databaseResource.Value.Data.EarliestRestoreOn;

                if (earliestRestore.HasValue)
                {
                    return earliestRestore.Value.ToString("dd/MM/yyyy HH:mm");
                }

                return "No disponible";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fecha más antigua");
                return "Error";
            }
        }

        // OPCIÓN 2: BACKUPS MANUALES CON BACPAC es más simple, no requiere Azure AD

        //public async Task<ResultadoOperacion> CrearBackupManualBacpacAsync(string nombreBackup)
        //{
        //    try
        //    {
        //        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //        var nombreArchivo = $"{nombreBackup}_{timestamp}.bacpac";


        //        // Leer el nombre del contenedor de backups desde la nueva sección de configuración
        //        var contenedorBackups = _configuration["AzureBackupStorage:ContainerName"];

        //        // Asumo que esta clave "Azure:StorageAccount" existe en tus secrets o settings.
        //        var storageAccountName = _configuration["AzureBackupStorage:StorageAccount"];


        //        // Usar Azure CLI (debe estar instalado en el servidor)
        //        var command = "az";
        //        var arguments = $"sql db export " +
        //                      $"--resource-group {_resourceGroup} " +
        //                      $"--server {_serverName} " +
        //                      $"--name {_databaseName} " +
        //                      $"--admin-user {_configuration["Azure:SqlUser"]} " +
        //                      $"--admin-password {_configuration["Azure:SqlPassword"]} " +
        //                      $"--storage-key {_configuration["Azure:StorageKey"]} " +
        //                      $"--storage-key-type StorageAccessKey " +
        //                      $"--storage-uri https://{storageAccountName}.blob.core.windows.net/{contenedorBackups}/{nombreArchivo}";

        //        var processInfo = new ProcessStartInfo
        //        {
        //            FileName = command,
        //            Arguments = arguments,
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        };

        //        using var process = Process.Start(processInfo);
        //        var output = await process.StandardOutput.ReadToEndAsync();
        //        var error = await process.StandardError.ReadToEndAsync();
        //        await process.WaitForExitAsync();

        //        if (process.ExitCode != 0)
        //        {
        //            _logger.LogError($"Error al crear backup: {error}");
        //            return new ResultadoOperacion
        //            {
        //                Exito = false,
        //                Mensaje = "Error al crear backup. Verifique los logs."
        //            };
        //        }

        //        // Guardar metadata en tabla
        //        // (implementar según necesites)

        //        return new ResultadoOperacion
        //        {
        //            Exito = true,
        //            Mensaje = $"Backup '{nombreArchivo}' creado exitosamente"
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al crear backup manual");
        //        return new ResultadoOperacion
        //        {
        //            Exito = false,
        //            Mensaje = $"Error: {ex.Message}"
        //        };
        //    }
        //}

        public async Task<ResultadoOperacion> CrearBackupManualBacpacAsync(string nombreBackup)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                // Añade un GUID para asegurar unicidad absoluta, incluso si ejecutas en el mismo segundo
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var nombreArchivo = $"{nombreBackup}_{timestamp}_{uniqueId}.bacpac";

                // Obtener la SAS Token para el Blob Storage
                var storageUrl = await GetBlobSasUrlAsync(nombreArchivo);

                // Autenticación con Azure Resource Manager usando DefaultAzureCredential
                // DefaultAzureCredential intentará usar la Managed Identity del App Service
                var credential = new DefaultAzureCredential();
                var armClient = new ArmClient(credential);

                // Identificar el recurso de la base de datos SQL
                var scope = new ResourceIdentifier($"/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroup}/providers/Microsoft.Sql/servers/{_serverName}/databases/{_databaseName}");
                var sqlDatabase = armClient.GetSqlDatabaseResource(scope);

                // Definir los parámetros de exportación
                var exportDefinition = new DatabaseExportDefinition(
                    storageKeyType: StorageKeyType.SharedAccessKey, // Usamos SAS Key generado
                    storageKey: storageUrl.SasToken,
                    storageUri: storageUrl.BlobUri,
                    administratorLogin: _sqlUser,
                    administratorLoginPassword: _sqlPassword)
                {
                    AuthenticationType = "Sql" // Especificamos autenticación SQL para la DB interna
                };

                // Iniciar la operación de exportación asíncrona
                // WaitUntil.Completed indica que el método esperará a que la operación termine
                ArmOperation<ImportExportOperationResult> lro = await sqlDatabase.ExportAsync(WaitUntil.Completed, exportDefinition);
                ImportExportOperationResult result = lro.Value;


                _logger.LogInformation($"Backup '{nombreArchivo}' creado exitosamente. Status: {result.Status}");

                return new ResultadoOperacion
                {
                    Exito = true,
                    Mensaje = $"Backup '{nombreArchivo}' creado exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup manual con Azure SDK.");
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = $"Error al crear backup: {ex.Message}"
                };
            }
        }

        // Helper para generar una SAS Token temporal necesaria para que el servicio de Exportación de SQL acceda al Blob
        private async Task<(Uri BlobUri, string SasToken)> GetBlobSasUrlAsync(string blobName)
        {
            var blobServiceClient = new BlobServiceClient(
                new Uri($"https://{_storageAccountName}.blob.core.windows.net/"),
                new DefaultAzureCredential());

            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);

            // Obtener User Delegation Key
            var userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
                DateTimeOffset.UtcNow.AddMinutes(-5),
                DateTimeOffset.UtcNow.AddHours(24) // Más tiempo para operaciones largas
            );

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
            };

            // Incluir Delete es OBLIGATORIO para Azure SQL Export
            sasBuilder.SetPermissions(
                BlobSasPermissions.Read |
                BlobSasPermissions.Write |
                BlobSasPermissions.Create |
                BlobSasPermissions.Delete |
                BlobSasPermissions.Add  // También recomendado
            );

            var sasQueryParameters = sasBuilder.ToSasQueryParameters(
                userDelegationKey,
                blobServiceClient.AccountName
            );

            // Retornar URI SIN el token SAS incluido
            var blobUriWithoutSas = blobClient.Uri; // URI limpia sin query string
            var sasTokenOnly = sasQueryParameters.ToString(); // Solo el token

            return (blobUriWithoutSas, sasTokenOnly);
        }


        // LISTAR BACKUPS MANUALES (BACPAC en Blob Storage)
        public async Task<List<BackupManualDto>> ListarBackupsManualesAsync()
        {
            try
            {
                //DefaultAzureCredential igual que en GetBlobSasUrlAsync
                var blobServiceClient = new BlobServiceClient(
                    new Uri($"https://{_storageAccountName}.blob.core.windows.net/"),
                    new DefaultAzureCredential()
                );

                // obtener referencia al contenedor
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                // Verificar que el contenedor existe
                if (!await containerClient.ExistsAsync())
                {
                    _logger.LogWarning($"El contenedor '{_containerName}' no existe");
                    return new List<BackupManualDto>();
                }

                var backups = new List<BackupManualDto>();

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    if (blobItem.Name.EndsWith(".bacpac"))
                    {
                        var blobClient = containerClient.GetBlobClient(blobItem.Name);
                        var properties = await blobClient.GetPropertiesAsync();

                        backups.Add(new BackupManualDto
                        {
                            NombreArchivo = blobItem.Name,
                            FechaCreacion = properties.Value.CreatedOn.DateTime,
                            TamanoBytes = properties.Value.ContentLength,
                            UrlDescarga = blobClient.Uri.ToString()
                        });
                    }
                }

                _logger.LogInformation($"Se encontraron {backups.Count} backups en el contenedor '{_containerName}'");
                return backups.OrderByDescending(b => b.FechaCreacion).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar backups manuales");
                return new List<BackupManualDto>();
            }
        }
    }
}
