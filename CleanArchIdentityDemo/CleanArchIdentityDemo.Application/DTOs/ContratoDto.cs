using System;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class ContratoDto
    {
        public int IdContrato { get; set; }
        public string Descripcion { get; set; } = string.Empty; // Nombre del empleado
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string RutaDocumento { get; set; } = string.Empty;

        // Asignación al Proyecto
        public int ProyectoId { get; set; }
        public string? ProyectoNombre { get; set; }
    }
}
