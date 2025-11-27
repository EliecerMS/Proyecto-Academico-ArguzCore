namespace CleanArchIdentityDemo.Application.DTOs
{
    public class PuntoRestauracionDto
    {
        public string NombrePunto { get; set; }
        public DateTimeOffset? FechaCreacion { get; set; }
        public string TipoPunto { get; set; }
        public DateTimeOffset FechaMasAntigua { get; set; }
    }
}
