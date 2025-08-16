using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class UserService : IUserService
    {
        //son variables que asisten para administrar los usuarios y los roles
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                var currentRoles = await _userManager.GetRolesAsync(user);
                //remueve el rol actual del usuario
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                //reasigna el nuevo rol
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        public async Task CreateUserAsync(string email, string password, string role, string Nombre)
        {
            //objeto de tipo ApplicatioUser asignando valores a sus atributos
            var user = new ApplicationUser { UserName = email, Email = email, NombreCompleto = Nombre };
            // espera la creacion del usuario en la table de usuarios
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                //resultado exitoso valida si el usuario creado tiene un rol
                if (!await _roleManager.RoleExistsAsync(role))
                    //de no tenerlo se le crea el rol
                    await _roleManager.CreateAsync(new IdentityRole(role));
                // agrega el rol para el usuario
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _userManager.DeleteAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string idUser) // devuelve la lista de usuarios por eso retorna un IEnumerable (lista) de tipo UserDto exeptuando que no se muestre el admin que inicio sesión
        {
            var users = _userManager.Users.Where(u => u.Id != idUser).ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users) //para cada usuario en la lista de usuarios recupera sus datos
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Nombrempleto = user.NombreCompleto,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                });
            }

            return userDtos;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Nombrempleto = user.NombreCompleto,
                Email = user.Email,
                Role = roles.FirstOrDefault()
            };
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id); // encuentra el usuario
            if (user != null)// una vez encontrado edito los datos requeridos
            {
                user.Email = userDto.Email;
                user.UserName = userDto.Email;
                user.NombreCompleto = userDto.Nombrempleto;
                await _userManager.UpdateAsync(user);

                var currentRoles = await _userManager.GetRolesAsync(user); // busca el rol actual de usuario a editar
                await _userManager.RemoveFromRolesAsync(user, currentRoles); // remueve el rol actual del usuario
                await _userManager.AddToRoleAsync(user, userDto.Role); //asigna el nuevo rol para el usuario
            }
        }
    }
}
