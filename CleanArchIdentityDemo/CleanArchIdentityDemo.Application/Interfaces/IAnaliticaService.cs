using System.Threading.Tasks;
using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IAnaliticaService
    {
        Task<byte[]> GenerarPdfComparacionAsync(ComparacionAnaliticaDto comparacion);
    }
}
