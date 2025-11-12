using System;
using System.Collections.Generic;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class ReporteFinancieroDto
    {
        public int ProyectoId { get; set; }
        public string NombreProyecto { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public decimal PresupuestoPlanificado { get; set; }

        public decimal TotalEjecutado { get; set; }

        public bool SobrepasoPresupuesto { get; set; }

        public decimal MontoSobrepaso { get; set; }

        public bool SobrepasoMayorDiezPorCiento { get; set; }

        public IEnumerable<CostoEjecutadoDto> CostosEjecutados { get; set; }
    }
}