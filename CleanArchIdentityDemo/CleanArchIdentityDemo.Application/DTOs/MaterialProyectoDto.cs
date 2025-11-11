namespace CleanArchIdentityDemo.Application.DTOs
{
    public class MaterialProyectoDto
    {
        public int IdMaterialProyecto { get; set; }

        public int ProyectoId { get; set; }

        public int MaterialId { get; set; }
        public string NombreMaterial { get; set; } = null!;

        public int CantidadEnObra { get; set; }

        public int CantidadEnBodega { get; set; }

    }
}
