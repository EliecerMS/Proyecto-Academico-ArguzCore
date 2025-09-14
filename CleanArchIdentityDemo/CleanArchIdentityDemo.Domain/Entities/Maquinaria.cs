using System.ComponentModel.DataAnnotations;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Maquinaria
    {
        // PK
        [Key]
        public int IdMaquinaria { get; set; }

        // Atributos
        public string CodigoMaquinaria { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;

        // Colecciones
        public ICollection<MantenimientoMaquinaria> Mantenimientos { get; set; } = new List<MantenimientoMaquinaria>();
        public ICollection<MaquinariaProyecto> ProyectosAsignados { get; set; } = new List<MaquinariaProyecto>();
    }
}
