using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CleanArchIdentityDemo.WebUI.Pages.JefeMaquinaria
{
    [Authorize(Roles = "JefeMaquinaria")]
    public class ListaMaquinariaModel : PageModel
    {
        private readonly IEquipoService _equipoService;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProyectoService _proyectoService;

        public ListaMaquinariaModel(
            IEquipoService equipoService,
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            IProyectoService proyectoService)
        {
            _equipoService = equipoService;
            _userService = userService;
            _userManager = userManager;
            _proyectoService = proyectoService;
        }

        public IEnumerable<MaquinariaDto> Maquinarias { get; set; } = new List<MaquinariaDto>();
        public List<string> NombresProyectos { get; set; } = new();
        [BindProperty]
        public MaquinariaDto NuevaMaquinaria { get; set; } = new MaquinariaDto();
        [BindProperty]
        public MaquinariaDto MaquinariaEditar { get; set; } = new MaquinariaDto();

        public async Task OnGetAsync()
        {
            Maquinarias = await _equipoService.GetAllAsync();

        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor complete todos los campos correctamente.";  // No debería ocurrir ya que todo lleva el atributo [Required]
                return Page();
            }

            try
            {
                await _equipoService.CreateAsync(NuevaMaquinaria);
                TempData["SuccessMessage"] = "Equipo creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error al crear el equipo: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _equipoService.UpdateAsync(MaquinariaEditar);
            if (result)
                TempData["SuccessMessage"] = "Equipo actualizado correctamente.";
            else
                TempData["ErrorMessage"] = "No se pudo actualizar el equipo.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _equipoService.DeleteAsync(id);
            if (resultado)
                TempData["SuccessMessage"] = "Equipo eliminado correctamente.";
            else
                TempData["ErrorMessage"] = "No se pudo eliminar el equipo.";

            return RedirectToPage();
        }

    }
}
