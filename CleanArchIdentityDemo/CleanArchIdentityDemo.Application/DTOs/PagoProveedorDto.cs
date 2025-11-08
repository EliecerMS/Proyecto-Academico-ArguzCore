namespace CleanArchIdentityDemo.Application.DTOs
{
    public class PagoProveedorDto
    {
        public int IdPago { get; set; }
        public int ProyectoId { get; set; }
        public int ProveedorId { get; set; }

        //por si se ocupa mostar el nombre del proovedor y del proyecto al que pertenece el pago del proveedor, ademas del contacto del proovedor
        public string NombreProveedor { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;
        public string ContactoProveedor { get; set; } = string.Empty;

        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string RutaComprobante { get; set; } = string.Empty;
        public string? NombreDocumentoSubido { get; set; } = string.Empty;
    }
}
