namespace CleanArchIdentityDemo.Application.DTOs
{
    public class ProyectoDashboardDto
    {
        //se definen las propiedades que se van a poder ver y editar de la tabla Proyecto
        public int IdProyecto { get; set; }
        public string CodigoProyecto { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int IdEstadoProyecto { get; set; }
        public string EstadoProyecto { get; set; } = string.Empty;
        public DateTime FechaFinalPropuesta { get; set; }
        public decimal Presupuesto { get; set; }
        public int desviacion { get; set; }

        public int PorcentajeAvance { get; set; }
    }
}
