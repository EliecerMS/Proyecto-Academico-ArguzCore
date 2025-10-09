using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(string email, string password, string role, string Nombre);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(string id);
        Task<UserDto> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UserDto user);
        Task DeleteUserAsync(string userId);
        Task AssignRoleAsync(string userId, string role);
        Task<IEnumerable<UserRolesDto>> GetRoles();

        //metodo que obtiene todos los usuarios con el rol usuario
        Task<IEnumerable<UserDto>> GetAllNormalUsersAsync();
    }
}
