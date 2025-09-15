using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IProyectoService
    {
        Task CrearProyectoAsync(ProyectoDto Proyecto);
        Task<IEnumerable<ProyectoDto>> MostrarProyectosAsync();
        Task<ProyectoDto> MostrarProyectoPorId(string IdProyecto);
        Task ActualizarProyectoAsync(ProyectoDto Proyecto);
        Task EliminarProyectoAsync(int IdProyecto);

        //Cualquier otros metodos relacionado al modulo de proyecto abajo

    }
}
