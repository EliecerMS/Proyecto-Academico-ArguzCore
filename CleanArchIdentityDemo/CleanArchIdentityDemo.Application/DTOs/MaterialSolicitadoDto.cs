using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class MaterialSolicitadoDto
    {
        public int IdMaterialSolicitado { get; set; }

         // Nombre del proyecto asociado a la solicitud
        public int SolicitudId { get; set; }
        public int MaterialId { get; set; }
        public int Cantidad { get; set; }
        public string Prioridad { get; set; } = string.Empty;

        // Propiedades opcionales para mostrar información relacionada (sin incluir entidades completas)
        public string? NombreMaterial { get; set; }     // Desde Material
        public string? TipoMaterial { get; set; }       // Desde Material
        public string? EstadoSolicitud { get; set; }    // Desde SolicitudMaterial (si lo necesitas)
        public string? NombreProyecto { get; set; }

        public int CantidadDisponible { get; set; } // Desde Material
    }
}
    
