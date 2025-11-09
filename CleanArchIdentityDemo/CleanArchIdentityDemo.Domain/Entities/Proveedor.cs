using System.ComponentModel.DataAnnotations;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Proveedor
    {
        // PK
        [Key]
        public int IdProveedor { get; set; }

        // Atributos
        public string NombreProveedor { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        // Colecciones
        public ICollection<Material> Materiales { get; set; } = new List<Material>();
        public ICollection<PagoProveedor> PagosProveedores { get; set; } = new List<PagoProveedor>();
    }
}
