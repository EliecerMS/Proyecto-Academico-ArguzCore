using System;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class MaquinariaProyectoDto
    {
        public int IdMaquinariaProyecto { get; set; }
        public int MaquinariaId { get; set; }
        public int ProyectoId { get; set; }
        public DateTime FechaAsignacion { get; set; }

    }
}
