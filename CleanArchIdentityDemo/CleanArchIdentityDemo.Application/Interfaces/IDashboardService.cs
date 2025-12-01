using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<byte[]> GenerarInformeProyectoAsync(int proyectoId);
    }
}
