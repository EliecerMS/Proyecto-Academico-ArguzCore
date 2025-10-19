using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CleanArchIdentityDemo.Domain.Entities;
using System.Collections.Generic;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class MaterialesService : IMaterialesService
    {
        private readonly ApplicationDbContext _context;

        public MaterialesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MaterialDto>> MostrarMaterialesAsync()
        {
            var materiales = await _context.Materiales.Include(m => m.Proveedor).ToListAsync();

            var resultado = materiales.Select(m => new MaterialDto
            {
                IdMaterial = m.IdMaterial,
                NombreMaterial = m.NombreMaterial,
                Tipo = m.Tipo,
                Descripcion = m.Descripcion,
                CantidadDisponible = m.CantidadDisponible,
                ProveedorId = m.ProveedorId
            }).ToList();

            return resultado;
        }

        public Task<MaterialDto> MostrarProyectoPorId(int IdMaterial)
        {
            throw new NotImplementedException();
        }

        public async Task RegistrarMaterialAsync(MaterialDto material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            bool existe = await _context.Materiales
                .AnyAsync(m => m.NombreMaterial.ToLower() == (material.NombreMaterial ?? string.Empty).Trim().ToLower());

            if (existe)
                throw new InvalidOperationException("Ya existe un material con ese nombre.");

            var entidad = new Domain.Entities.Material
            {
                NombreMaterial = material.NombreMaterial?.Trim() ?? string.Empty,
                Tipo = material.Tipo,
                Descripcion = material.Descripcion,
                CantidadDisponible = material.CantidadDisponible,
                ProveedorId = material.ProveedorId
            };

            _context.Materiales.Add(entidad);
            await _context.SaveChangesAsync();
        }

        // Devuelve proveedores como DTO para la UI
        public async Task<IEnumerable<ProveedorMaterialDto>> GetProveedoresAsync()
        {
            return await _context.Proveedores
                .OrderBy(p => p.NombreProveedor)
                .Select(p => new ProveedorMaterialDto
                {
                    idProveedor = p.IdProveedor,
                    NombreProveedor = p.NombreProveedor
                })
                .ToListAsync();
        }


        //Metodos para tab Solicitud Material
        public async Task<IEnumerable<MaterialSolicitadoDto>> MostrarMaterialesSolicitadosAsync()
        {
            var materiales = await _context.MaterialesSolicitados
                .Include(ms => ms.Material)
                .Include(ms => ms.SolicitudMaterial)
                    .ThenInclude(sm => sm.Proyecto)
                .ToListAsync();

            var resultado = materiales.Select(ms => new MaterialSolicitadoDto
            {
                IdMaterialSolicitado = ms.IdMaterialSolicitado,
                SolicitudId = ms.SolicitudId,
                MaterialId = ms.MaterialId,
                Cantidad = ms.Cantidad,
                Prioridad = ms.Prioridad,
                NombreMaterial = ms.Material?.NombreMaterial,
                TipoMaterial = ms.Material?.Tipo,
                EstadoSolicitud = ms.SolicitudMaterial.EstadoSolicitud,
                NombreProyecto = ms.SolicitudMaterial.Proyecto.Nombre

            }).ToList();

            return resultado;
        }

        public async Task<string> AceptarSolicitudAsync(int IdSolicitud, string Observaciones)
        {
            // Buscar la solicitud junto con el material y la cabecera de la solicitud
            var solicitud = await _context.MaterialesSolicitados
                .Include(s => s.Material)
                .Include(s => s.SolicitudMaterial)
                .FirstOrDefaultAsync(s => s.SolicitudId == IdSolicitud);

            if (solicitud == null)
                return "NO_EXISTE";

            if (solicitud.Cantidad > solicitud.Material.CantidadDisponible)
                return "EXCEDE";

            // Actualizar estado de la solicitud
            solicitud.SolicitudMaterial.EstadoSolicitud = "Aceptado";

            // Guardar las observaciones si el campo existe en tu modelo
            solicitud.SolicitudMaterial.ObservacionesBodeguero = Observaciones;

            // Reducir cantidad disponible del material
            solicitud.Material.CantidadDisponible -= solicitud.Cantidad;

            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            return "OK";
        }


        public async Task<string> RechazarSolicitudAsync(int IdSolicitud)
        {
            var solicitud = await _context.MaterialesSolicitados
                .Include(s => s.Material)
                .Include(s => s.SolicitudMaterial)
                .FirstOrDefaultAsync(s => s.SolicitudId == IdSolicitud);
            if (solicitud == null)
                return "NO_EXISTE";

            solicitud.SolicitudMaterial.EstadoSolicitud = "Rechazado";

            await _context.SaveChangesAsync();
            return "OK";
        }

        public async Task<MaterialSolicitadoDto> MostrarSolicitudPorIdAsync(int idSolicitud)
        {
            var solicitud = await _context.MaterialesSolicitados
       .Include(s => s.Material)
       .FirstOrDefaultAsync(s => s.SolicitudId == idSolicitud);

            if (solicitud == null)
                return null;

            return new MaterialSolicitadoDto
            {
                SolicitudId = solicitud.SolicitudId,
                MaterialId = solicitud.MaterialId,
                NombreMaterial = solicitud.Material.NombreMaterial,
                Cantidad = solicitud.Cantidad,
                Prioridad = solicitud.Prioridad,
                CantidadDisponible = solicitud.Material.CantidadDisponible
            };
        }
    }
}
