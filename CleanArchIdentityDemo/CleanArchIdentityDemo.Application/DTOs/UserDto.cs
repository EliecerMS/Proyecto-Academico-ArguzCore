namespace CleanArchIdentityDemo.Application.DTOs
{
    public class UserDto
    {
        // Creada en Application Dtos por ser una entidad no pura del negocio
        // atributos para guardado y que se podran editar
        public string Id { get; set; }
        public string Email { get; set; }
        public string Nombrempleto { get; set; }
        public string Role { get; set; }

        // solo se usa cuando se crea un usuario, no en consultas normales
        public string Password { get; set; }
    }
}
