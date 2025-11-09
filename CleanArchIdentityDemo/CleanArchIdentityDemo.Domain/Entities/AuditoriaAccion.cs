using System.ComponentModel.DataAnnotations;

namespace CleanArchIdentityDemo.Domain.Entities
{
    public class AuditoriaAccion
    {
        // PK
        [Key]
        public int IdAuditoria { get; set; }

        // FK -> Usuarios.IdUsuario
        public string? UsuarioId { get; set; }

       

        // Atributos
        public string Modulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;

        public string? DatoAnterior { get; set; } = string.Empty;  
        public string? DatoNuevo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }

    }
}
