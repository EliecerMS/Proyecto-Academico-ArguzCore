using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class ComparacionAnaliticaDto
    {
        public string NombreProyectoA { get; set; } = "";
        public string NombreProyectoB { get; set; } = "";

        public int? MesA { get; set; }
        public int? MesB { get; set; }

        public string EstadoProyectoA { get; set; } = "";
        public string EstadoProyectoB { get; set; } = "";

        public int AvanceFisicoA { get; set; }
        public int AvanceFisicoB { get; set; }

        public decimal PresupuestoEjecutadoPorcentajeA { get; set; }
        public decimal PresupuestoEjecutadoPorcentajeB { get; set; }

        public decimal DesviacionPorcentajeA { get; set; }
        public decimal DesviacionPorcentajeB { get; set; }

        public string RiesgoA { get; set; } = "";
        public string RiesgoB { get; set; } = "";
    }
}
