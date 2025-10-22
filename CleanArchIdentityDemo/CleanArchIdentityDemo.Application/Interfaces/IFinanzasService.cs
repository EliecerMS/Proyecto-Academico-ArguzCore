using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IFinanzasService
    {
        //metodos para realizar operaciones en la vista de Finanzas
        byte[] GenerarComprobante(int IdPago); //genera un recibo de pago en formato PDF y lo devuelve como un arreglo de bytes

        Task<PagoProveedorDto> DatosPagoProveedorPorIdAsync(int IdPago);
        Task<IEnumerable<PagoProveedorDto>> ListarPagosProveedoresAsync(); //lista todos los pagos a proveedores

        Task<IEnumerable<ProveedorDto>> ListarProveedoresAsync(); //lista todos los proveedores

        Task<bool> CrearProveedorAsync(ProveedorDto DatosProveedor);

        Task<bool> EditarProveedorAsync(ProveedorDto DatosProveedor);

        Task<bool> EliminarProveedorAsync(int IdProveedor);
        //Método para registrar pago a proveedor
        Task<PagoProveedorDto> RegistrarPagoProveedorAsync(PagoProveedorDto dto);

        //Método para listar proyecto
        Task<IEnumerable<ProyectoDto>> ListarProyectosAsync();
    }
}
