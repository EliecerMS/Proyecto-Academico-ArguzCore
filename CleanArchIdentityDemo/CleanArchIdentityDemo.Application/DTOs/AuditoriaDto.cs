using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class AuditoriaDto
    {
        public int IdAuditoria { get; set; }
        public string UsuarioId { get; set; }

        public string NombreUsuario { get; set; }

        // Atributos
        public string Modulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;

        public string? DatoAnterior { get; set; } = string.Empty;
        public string? DatoNuevo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
    }
}
