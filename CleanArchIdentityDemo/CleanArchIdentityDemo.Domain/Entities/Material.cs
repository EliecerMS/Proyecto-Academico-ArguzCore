using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Material
    {
        // PK
        [Key]
        public int IdMaterial { get; set; }

        // Atributos
        public string NombreMaterial { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }

        // FK -> Proveedores.IdProveedor
        public int ProveedorId { get; set; }

        // Navegación
        [ForeignKey(nameof(ProveedorId))]
        public Proveedor Proveedor { get; set; } = null!;

        // Colecciones
        public ICollection<MaterialSolicitado> MaterialesSolicitados { get; set; } = new List<MaterialSolicitado>();
        public ICollection<MaterialProyecto> MaterialesProyecto { get; set; } = new List<MaterialProyecto>();
    }
}
