namespace CleanArchIdentityDemo.Application.DTOs
{
    public class MantenimientoMaquinariaDto
    {
        public int IdMantenimiento { get; set; }

        public int MaquinariaId { get; set; }

        public DateTime FechaProgramada { get; set; }

        public DateTime FechaCompletado { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}