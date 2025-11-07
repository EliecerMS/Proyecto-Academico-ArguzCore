using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchIdentityDemo.WebUI.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class AdminModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminModel(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public List<UserDto> Users { get; set; } = new(); // almacenara la lista de usuarios

        public List<UserRolesDto> Roles { get; set; } = new(); // lista de roles disponibles

        // [BindProperty] es un atributo de Razor Pages que indica que una propiedad del PageModel debe enlazarse automáticamente a los datos que vienen de la solicitud HTTP, ya sea vía formulario (POST) o query string (GET).permite recibir datos del formulario en tu página sin tener que leerlos manualmente del Request.Form.

        [BindProperty]
        public UserDto NewUser { get; set; } = new();

        [BindProperty]
        public UserDto EditedUser { get; set; } = new();

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string IdUsuario { get; set; } = string.Empty;

        public async Task OnGetAsync()//muestra los usuarios excepto el del admin actual
        {
            var idUser = _userManager.GetUserId(User);
            if (idUser != null)
            {
                Users = (await _userService.GetAllUsersAsync(idUser)).ToList();
                //carga la lista de roles
                Roles = (List<UserRolesDto>)await _userService.GetRoles();
            }
            //TempData["TabActiva"] = "Usuarios";

        }

        public async Task<IActionResult> OnPostCreateAsync() // crear un nuevo usuario
        {
            try
            {
                bool resultado = await _userService.CreateUserAsync(NewUser.Email, NewUser.Password, NewUser.Role, NewUser.NombreCompleto);
                if (!resultado)
                {
                    TempData["ErrorMessage"] = "Ya hay un usuario con ese correo, intente de nuevo";

                }
                else
                {
                    TempData["SuccessMessage"] = "Se creó el usuario correctamente";
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, mostrar un mensaje de error)
                TempData["ErrorMessage"] = "Error al crear el usuario";
            }

            return RedirectToPage(); // Recargar la página
        }

        public async Task<IActionResult> OnPostEditAsync() //edita el usuario
        {
            try
            {

                await _userService.UpdateUserAsync(EditedUser);
                TempData["SuccessMessage"] = "Se editó el usuario correctamente";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, mostrar un mensaje de error)
                TempData["ErrorMessage"] = "Error al editar el usuario";
            }
            return RedirectToPage();
        }

        public async Task<ActionResult> OnPostDeleteAsync() //elimina el usuario
        {
            try
            {
                await _userService.DeleteUserAsync(IdUsuario);
                TempData["SuccessMessage"] = "Se desactivó el usuario correctamente";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, mostrar un mensaje de error)
                TempData["ErrorMessage"] = "Error al eliminar el usuario";
            }

            return RedirectToPage();
        }

        public async Task<ActionResult> OnPostActivateAsync() //se activa el usuario
        {
            try
            {
                await _userService.ActivateUserAsync(IdUsuario);
                TempData["SuccessMessage"] = "Se activó el usuario correctamente";
            }
            catch (Exception ex)
            {
                // Manejar la excepción (por ejemplo, mostrar un mensaje de error)
                TempData["ErrorMessage"] = "Error al activar el usuario";
            }

            return RedirectToPage();
        }
    }
}
