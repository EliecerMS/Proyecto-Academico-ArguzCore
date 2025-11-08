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
    }
}
