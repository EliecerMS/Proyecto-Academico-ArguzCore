using System;
using System.Collections.Generic;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class SolicitudMaterialDto
    {
        public int IdSolicitud { get; set; }
        public int ProyectoId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string EstadoSolicitud { get; set; } = "Abierta";
        public string? ObservacionesBodeguero { get; set; }

        public List<MaterialSolicitadoDto> MaterialesSolicitados { get; set; } = new();
    }

    public class MaterialSolicitadoDto
    {
        public int IdMaterialSolicitado { get; set; }
        public int MaterialId { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Prioridad { get; set; } = "Media";
    }
}
