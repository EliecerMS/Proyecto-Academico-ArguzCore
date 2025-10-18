using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class MaterialDto
    {
        public int IdMaterial { get; set; }
<<<<<<< HEAD
        public string NombreMaterial { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public int CantidadDisponible { get; set; }
        public int ProveedorId { get; set; }
=======
        public string NombreMaterial { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }
>>>>>>> main
    }
}
