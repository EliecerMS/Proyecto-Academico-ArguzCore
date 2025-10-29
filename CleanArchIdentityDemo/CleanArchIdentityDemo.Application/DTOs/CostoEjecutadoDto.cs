using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class CostoEjecutadoDto
    {
        public int IdCosto { get; set; }

        public int ProyectoId { get; set; }

        public string CategoriaGasto { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public string RutaComprobante { get; set; } = string.Empty;

        public string NombreProyecto { get; set; } = string.Empty;

        public bool ArchivoActualizado { get; set; } = false;
    }
}