using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IAuditoriaService
    {

        Task<IEnumerable<AuditoriaDto>> MostrarRegistrosAsync();
        Task<List<AccesoModuloDto>> ObtenerAccesosPorModuloAsync(string modulo, string usuarioId);
        Task<List<AccesoModuloDto>> ObtenerAccesosPorUsuarioAsync(string usuarioId);
        Task<byte[]> GenerarReportePdfAsync(List<AccesoModuloDto> datos, string titulo = "Reporte de Accesos a Módulos");
        Task RegistrarAccesoAsync(string modulo);

    }
}
