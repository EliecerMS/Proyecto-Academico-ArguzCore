using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class MaquinariaDto
    {
        public int IdMaquinaria { get; set; }
        public string CodigoMaquinaria { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;


        public List<MantenimientoMaquinariaDto> Mantenimientos { get; set; } = new();
        public List<MaquinariaProyectoDto> ProyectosAsignados { get; set; } = new();

    }
}
