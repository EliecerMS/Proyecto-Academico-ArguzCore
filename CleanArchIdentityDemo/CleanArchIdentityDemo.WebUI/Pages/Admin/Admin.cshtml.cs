using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using CleanArchIdentityDemo.Infrastructure.Services;
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
        private readonly IAuditoriaService _auditoriaService;
        

        public AdminModel(IUserService userService, UserManager<ApplicationUser> userManager, IAuditoriaService auditoria)
        private readonly IBDRespladosService _BDRespaldoService;
        private readonly IBlobStorageService _blobStorageService;

        public AdminModel(IUserService userService, UserManager<ApplicationUser> userManager, IBDRespladosService BDRespladosService, IBlobStorageService blobStorageService)
        {
            _userService = userService;
            _userManager = userManager;
            _auditoriaService = auditoria;
            _BDRespaldoService = BDRespladosService;
            _blobStorageService = blobStorageService;
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

        //Propiedas para respaldo de BD
        public List<PuntoRestauracionDto> PuntosRestauracion { get; set; } = new();
        public List<BackupManualDto> BackupsManuales { get; set; } = new();

        [BindProperty]
        public string UrlBackupEliminar { get; set; }

        [BindProperty]
        public string NombreBackup { get; set; }

        public async Task OnGetAsync()//muestra los usuarios excepto el del admin actual
        {
            var idUser = _userManager.GetUserId(User);
            if (idUser != null)
            {
                Users = (await _userService.GetAllUsersAsync(idUser)).ToList();
                //carga la lista de roles
                Roles = (List<UserRolesDto>)await _userService.GetRoles();
                await _auditoriaService.RegistrarAccesoAsync("Administracion");
            }

            //Registra el acceso de los usuarios y lo guarda en la tabla de auditoria
            
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

        public async Task<IActionResult> OnGetCargarBackupsAutomaticosAsync()
        {
            // Obtener la fecha del backup más antiguo
            //FechaBackupMasAntiguo = await _BDRespaldoService.ObtenerFechaBackupMasAntiguoAsync();
            // Obtener la lista de puntos de restauración (backups automáticos)
            PuntosRestauracion = (await _BDRespaldoService.ListarPuntosRestauracionAsync());


            // Retornar solo la partial view
            return Partial("_TablaBackupsPITR", PuntosRestauracion.ToList());

        }

        public async Task<IActionResult> OnGetCargarBackupsManualesAsync()
        {

            // Obtener la lista de puntos de restauración (backups automáticos)
            BackupsManuales = (await _BDRespaldoService.ListarBackupsManualesAsync());


            // Retornar solo la partial view
            return Partial("_TablaRespaldosManuales", BackupsManuales.ToList());

        }

        public async Task<IActionResult> OnPostCrearBackupManual([FromBody] string nombreBackup)
        {
            try
            {
                var resultado = await _BDRespaldoService.CrearBackupManualBacpacAsync(nombreBackup);

                if (resultado.Exito)
                {
                    // Devolver una respuesta JSON de éxito
                    return new JsonResult(new { success = true, mensaje = resultado.Mensaje });
                }
                else
                {
                    // Devolver error JSON (código 400 Bad Request)
                    return new BadRequestObjectResult(new { success = false, message = resultado.Mensaje });
                }
            }
            catch (Exception ex)
            {
                //
                // Devolver un error interno del servidor
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> OnGetDescargarBackupAsync(string urlDescarga, string nombreBackup)
        {
            try
            {
                // 2. Descargar archivo desde Blob Storage
                var (stream, contentType) = await _blobStorageService.DescargarBackupAsync(urlDescarga!);

                // 3. Retornar archivo al navegador
                return File(stream, contentType, nombreBackup!);
            }
            catch (FileNotFoundException ex)
            {
                TempData["ErrorMessage"] = "Backup no encontrado";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al descargar el archivo backup";

            }

            TempData["TabActiva"] = "RespaldosManuales";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarBackupAsync()
        {
            var resultado = await _blobStorageService.EliminarBackupAsync(UrlBackupEliminar);
            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
            }
            else
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
            }

            return RedirectToPage();
        }
    }
}
