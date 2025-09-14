using System.ComponentModel.DataAnnotations;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class AccesoModulo
    {
        // PK
        [Key]
        public int IdAccesoModulo { get; set; }

        // FK -> Usuarios.IdUsuario
        public string UsuarioId { get; set; }

        // Atributos
        public string Modulo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }

    }
}
