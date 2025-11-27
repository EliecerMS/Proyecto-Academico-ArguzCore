using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class Proyecto
    {
        // PK
        [Key]
        public int IdProyecto { get; set; }

        // Atributos
        public string CodigoProyecto { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaFinalPropuesta { get; set; }
        public DateTime? FechaInicioPropuesta { get; set; }
        public decimal Presupuesto { get; set; }
        public bool Activo { get; set; } = true;
        // FK -> EstadosProyecto.IdEstadoProyecto
        public int EstadoProyectoId { get; set; }


        // Navegación hacia EstadoProyecto (padre)
        [ForeignKey(nameof(EstadoProyectoId))]
        public EstadoProyecto EstadoProyecto { get; set; } = null!;


        // Colecciones (hijos)
        public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
        public ICollection<PersonalProyecto> PersonalProyectos { get; set; } = new List<PersonalProyecto>();
        public ICollection<NotaAvance> NotasAvance { get; set; } = new List<NotaAvance>();
        public ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();
        public ICollection<SolicitudMaterial> SolicitudesMaterial { get; set; } = new List<SolicitudMaterial>();
        public ICollection<MaterialProyecto> MaterialesProyecto { get; set; } = new List<MaterialProyecto>();
        public ICollection<CostoEjecutado> CostosEjecutados { get; set; } = new List<CostoEjecutado>();
        public ICollection<PagoProveedor> PagosProveedores { get; set; } = new List<PagoProveedor>();
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
        public ICollection<MaquinariaProyecto> MaquinariasProyecto { get; set; } = new List<MaquinariaProyecto>();
    }
}
