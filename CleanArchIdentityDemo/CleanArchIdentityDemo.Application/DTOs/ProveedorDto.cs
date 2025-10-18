using System.ComponentModel.DataAnnotations;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class ProveedorDto
    {


        public int IdProveedor { get; set; }

        [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Entre 3 y 100 caracteres")]
        public string NombreProveedor { get; set; }

        [Required(ErrorMessage = "El contacto es obligatorio")]
        public string Contacto { get; set; }
    }
}
