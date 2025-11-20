using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IBDRespladosService
    {
        //Metodos para respaldos de BD
        Task<List<PuntoRestauracionDto>> ListarPuntosRestauracionAsync();
        Task<string> ObtenerFechaBackupMasAntiguoAsync();
        Task<ResultadoOperacion> CrearBackupManualBacpacAsync(string nombreBackup);
        Task<List<BackupManualDto>> ListarBackupsManualesAsync();
    }
}
