using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class ProyectoService : IProyectoService
    {
        public Task ActualizarProyectoAsync(ProyectoDto Proyecto)
        {
            throw new NotImplementedException();
        }

        public Task CrearProyectoAsync(ProyectoDto Proyecto)
        {
            throw new NotImplementedException();
        }

        public Task EliminarProyectoAsync(int IdProyecto)
        {
            throw new NotImplementedException();
        }

        public Task<ProyectoDto> MostrarProyectoPorId(string IdProyecto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProyectoDto>> MostrarProyectosAsync()
        {
            throw new NotImplementedException();
        }
    }
}
