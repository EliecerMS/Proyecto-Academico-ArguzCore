using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class AdminModel : PageModel
    {
        private readonly IUserService _userService;

        public AdminModel(IUserService userService)
        {
            _userService = userService;
        }

        public List<UserDto> Users { get; set; } = new(); // almacenara la lista de usuarios

        // [BindProperty] es un atributo de Razor Pages que indica que una propiedad del PageModel debe enlazarse automáticamente a los datos que vienen de la solicitud HTTP, ya sea vía formulario (POST) o query string (GET).permite recibir datos del formulario en tu página sin tener que leerlos manualmente del Request.Form.

        [BindProperty]
        public UserDto NewUser { get; set; } = new();

        [BindProperty]
        public UserDto EditedUser { get; set; } = new();

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        public async Task OnGetAsync()//muestra los usuarios
        {
            Users = (await _userService.GetAllUsersAsync()).ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync() // crear un nuevo usuario
        {

            await _userService.CreateUserAsync(NewUser.Email, NewUser.Password, NewUser.Role, NewUser.Nombrempleto);

            return RedirectToPage(); // Recargar la página
        }

        public async Task<IActionResult> OnPostEditAsync() //edita el usuario
        {

            await _userService.UpdateUserAsync(EditedUser);
            return RedirectToPage();
        }
    }
}
