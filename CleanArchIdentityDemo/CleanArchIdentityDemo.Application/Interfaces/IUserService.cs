using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IUserService
    {
        Task CreateUserAsync(string email, string password, string role, string Nombre);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UserDto user);
        Task DeleteUserAsync(string userId);
        Task AssignRoleAsync(string userId, string role);
    }
}
