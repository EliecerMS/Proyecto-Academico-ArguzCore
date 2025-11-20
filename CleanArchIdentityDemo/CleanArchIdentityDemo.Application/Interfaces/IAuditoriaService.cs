using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IAuditoriaService
    {

        Task<IEnumerable<AuditoriaDto>> MostrarRegistrosAsync();
    }
}
