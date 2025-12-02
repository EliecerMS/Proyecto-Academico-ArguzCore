using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class AccesoModuloDto
    {

        public string? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string Modulo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}


