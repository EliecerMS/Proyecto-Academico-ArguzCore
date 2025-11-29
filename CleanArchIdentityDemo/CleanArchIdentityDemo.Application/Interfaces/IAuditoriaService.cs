using CleanArchIdentityDemo.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
