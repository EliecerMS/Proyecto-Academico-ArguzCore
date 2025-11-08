using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IContratoService
    {
        Task<IEnumerable<ContratoDto>> GetAllAsync();
        Task<ContratoDto?> GetByIdAsync(int id);
        Task<ContratoDto> CreateAsync(ContratoDto dto);
        Task<bool> UpdateAsync(ContratoDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
