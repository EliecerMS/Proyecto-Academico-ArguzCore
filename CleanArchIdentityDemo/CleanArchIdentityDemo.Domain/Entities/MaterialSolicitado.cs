using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class MaterialSolicitado
    {
        // PK
        [Key]
        public int IdMaterialSolicitado { get; set; }

        // FK -> SolicitudesMaterial.IdSolicitud
        public int SolicitudId { get; set; }

        // FK -> Materiales.IdMaterial
        public int MaterialId { get; set; }

        // Atributos
        public int Cantidad { get; set; }
        public string Prioridad { get; set; } = string.Empty;

        // Navegaciones
        [ForeignKey(nameof(SolicitudId))]
        public SolicitudMaterial SolicitudMaterial { get; set; } = null!;

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; } = null!;
    }
}
