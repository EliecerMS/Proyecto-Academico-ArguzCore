using CleanArchIdentityDemo.Application.DTOs;


namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IEquipoService
    {
        //metodos para realizar operaciones en la vista de Equipo (basado en el prototipo, metodos para vistas ListaMaquinaria y DetallesMantenimiento)

        Task<IEnumerable<MaquinariaDto>> GetAllAsync();
        Task<MaquinariaDto?> GetByIdAsync(int id);
        Task<MaquinariaDto> CreateAsync(MaquinariaDto dto);
        Task<bool> UpdateAsync(MaquinariaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> AsignarProyectoAsync(int idMaquinaria, string nombreProyecto); // Nuevo método para asignar un proyecto a una maquinaria, este está fuera de las HU actuales


    }
}
