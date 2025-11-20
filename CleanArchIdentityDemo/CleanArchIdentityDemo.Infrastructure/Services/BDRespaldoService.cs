using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Sql;
using Azure.Storage.Blobs;
using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class BDRespaldoService : IBDRespladosService
    {
        private readonly string _subscriptionId;
        private readonly string _resourceGroup;
        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuditoriaService> _logger;

        public BDRespaldoService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuditoriaService> logger)
        {
            //logger de auditoría
            _logger = logger;

            // Configuración de Azure
            _subscriptionId = configuration["Azure:SubscriptionId"];
            _resourceGroup = configuration["Azure:ResourceGroup"];
            _serverName = configuration["Azure:ServerName"];
            _databaseName = configuration["Azure:DatabaseName"];
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

        public async Task<ResultadoOperacion> CrearBackupManualBacpacAsync(string nombreBackup)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"{nombreBackup}_{timestamp}.bacpac";


                // Leer el nombre del contenedor de backups desde la nueva sección de configuración
                var contenedorBackups = _configuration["AzureBackupStorage:ContainerName"];

                // Asumo que esta clave "Azure:StorageAccount" existe en tus secrets o settings.
                var storageAccountName = _configuration["AzureBackupStorage:StorageAccount"];


                // Usar Azure CLI (debe estar instalado en el servidor)
                var command = "az";
                var arguments = $"sql db export " +
                              $"--resource-group {_resourceGroup} " +
                              $"--server {_serverName} " +
                              $"--name {_databaseName} " +
                              $"--admin-user {_configuration["Azure:SqlUser"]} " +
                              $"--admin-password {_configuration["Azure:SqlPassword"]} " +
                              $"--storage-key {_configuration["Azure:StorageKey"]} " +
                              $"--storage-key-type StorageAccessKey " +
                              $"--storage-uri https://{storageAccountName}.blob.core.windows.net/{contenedorBackups}/{nombreArchivo}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"Error al crear backup: {error}");
                    return new ResultadoOperacion
                    {
                        Exito = false,
                        Mensaje = "Error al crear backup. Verifique los logs."
                    };
                }

                // Guardar metadata en tabla
                // (implementar según necesites)

                return new ResultadoOperacion
                {
                    Exito = true,
                    Mensaje = $"Backup '{nombreArchivo}' creado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup manual");
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }


        // LISTAR BACKUPS MANUALES (BACPAC en Blob Storage)
        public async Task<List<BackupManualDto>> ListarBackupsManualesAsync()
        {
            try
            {
                var blobConnectionString = _configuration.GetConnectionString("BlobStorageConnection");
                var blobServiceClient = new BlobServiceClient(blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("backups");

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
