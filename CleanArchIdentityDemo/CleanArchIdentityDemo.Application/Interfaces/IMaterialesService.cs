using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IMaterialesService
    {
        //metodos para realizar operaciones en la vista de Materiales

        Task<MaterialDto> MostrarProyectoPorId(int IdMaterial);
        Task<IEnumerable<MaterialDto>> MostrarMaterialesAsync();
        Task RegistrarMaterialAsync(MaterialDto material);
        Task EditarMaterialAsync(MaterialDto material);
        Task EliminarMaterialAsync(int idMaterial);

        //Metodos para solicitudes de materiales
        Task<IEnumerable<MaterialSolicitadoDto>> MostrarMaterialesSolicitadosAsync();
        Task<MaterialSolicitadoDto> MostrarSolicitudPorIdAsync(int idSolicitud);
        Task<string> AceptarSolicitudAsync(int idSolicitud, string Observaciones);
        Task<string> RechazarSolicitudAsync(int idSolicitud, string Observaciones);



        // Obtener proveedores en formato DTO 
        Task<IEnumerable<ProveedorMaterialDto>> GetProveedoresAsync();
    }
}
