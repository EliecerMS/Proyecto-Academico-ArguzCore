using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IFinanzasService
    {
        //metodos para realizar operaciones en la vista de Finanzas
        byte[] GenerarComprobante(int IdPago); //genera un recibo de pago en formato PDF y lo devuelve como un arreglo de bytes

        Task<PagoProveedorDto> DatosPagoProveedorPorIdAsync(int IdPago);
        Task<IEnumerable<PagoProveedorDto>> ListarPagosProveedoresAsync(); //lista todos los pagos a proveedores
    }
}
