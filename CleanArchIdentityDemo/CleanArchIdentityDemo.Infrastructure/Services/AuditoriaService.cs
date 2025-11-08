using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditoriaService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        public async Task<IEnumerable<AuditoriaDto>> MostrarRegistrosAsync()
        {
            var auditorias = await _context.AuditoriaAcciones
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            var result = new List<AuditoriaDto>();

            foreach (var a in auditorias)
            {
                var user = await _userManager.FindByIdAsync(a.UsuarioId ?? "");
                result.Add(new AuditoriaDto
                {
                    IdAuditoria = a.IdAuditoria,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = user?.UserName ?? "Sistema",
                    Modulo = a.Modulo,
                    Accion = a.Accion,
                    DatoAnterior = a.DatoAnterior,
                    DatoNuevo = a.DatoNuevo,
                    FechaHora = a.FechaHora
                });
            }

            return result;
        }

    }
}
