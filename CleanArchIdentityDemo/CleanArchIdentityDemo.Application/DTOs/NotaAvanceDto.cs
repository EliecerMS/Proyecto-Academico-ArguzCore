using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class NotaAvanceDto
    {
        public int IdNota { get; set; } 

        public int ProyectoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Destacada { get; set; }
        public DateTime FechaNota { get; set; }
        public string CreadoPor { get; set; } = string.Empty;
            
    }
}
