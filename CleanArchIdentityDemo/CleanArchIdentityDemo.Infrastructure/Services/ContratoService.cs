using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class ContratoService : IContratoService
    {
        private readonly ApplicationDbContext _context;

        public ContratoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listar todos los contratos laborales
        public async Task<IEnumerable<ContratoDto>> GetAllAsync()
        {
            return await _context.Contratos
                .AsNoTracking()
                .Include(c => c.Proyecto)
                .Select(c => new ContratoDto
                {
                    IdContrato = c.IdContrato,
                    Descripcion = c.Descripcion,     // nombre del empleado
                    FechaInicio = c.FechaInicio,
                    FechaFin = c.FechaFin,
                    RutaDocumento = c.RutaDocumento ?? string.Empty,
                    ProyectoId = c.ProyectoId,
                    ProyectoNombre = c.Proyecto != null ? c.Proyecto.Nombre : null
                })
                .ToListAsync();
        }

        // Obtener contrato por Id (detalles)
        public async Task<ContratoDto?> GetByIdAsync(int id)
        {
            var c = await _context.Contratos
                .AsNoTracking()
                .Include(x => x.Proyecto)
                .FirstOrDefaultAsync(x => x.IdContrato == id);

            if (c == null) return null;

            return new ContratoDto
            {
                IdContrato = c.IdContrato,
                Descripcion = c.Descripcion,
                FechaInicio = c.FechaInicio,
                FechaFin = c.FechaFin,
                RutaDocumento = c.RutaDocumento ?? string.Empty,
                ProyectoId = c.ProyectoId,
                ProyectoNombre = c.Proyecto?.Nombre
            };
        }

        // Crear contrato laboral
        public async Task<ContratoDto> CreateAsync(ContratoDto dto)
        {
            if (dto.FechaFin < dto.FechaInicio)
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");

            // Valida FK de proyecto para evitar DbUpdateException
            var proyectoExiste = await _context.Proyectos
                .AsNoTracking()
                .AnyAsync(p => p.IdProyecto == dto.ProyectoId);

            if (!proyectoExiste)

                throw new ArgumentException("El proyecto especificado no existe.");

            var entity = new Contrato
            {
                Descripcion = dto.Descripcion,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                RutaDocumento = dto.RutaDocumento,
                ProyectoId = dto.ProyectoId
            };

            _context.Contratos.Add(entity);
            await _context.SaveChangesAsync();

            // Opcional: devolver nombre del proyecto para la vista
            string? proyectoNombre = await _context.Proyectos
                .Where(p => p.IdProyecto == entity.ProyectoId)
                .Select(p => p.Nombre)
                .FirstOrDefaultAsync();

            dto.IdContrato = entity.IdContrato;
            dto.RutaDocumento = entity.RutaDocumento ?? string.Empty;
            dto.ProyectoNombre = proyectoNombre;

            return dto;
        }

        // Actualizar contrato laboral
        public async Task<bool> UpdateAsync(ContratoDto dto)
        {
            var entity = await _context.Contratos.FindAsync(dto.IdContrato);
            if (entity == null) return false;

            if (dto.FechaFin < dto.FechaInicio)
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");

            // Si cambia de proyecto, valida que exista
            if (entity.ProyectoId != dto.ProyectoId)
            {
                var existe = await _context.Proyectos
                    .AsNoTracking()
                    .AnyAsync(p => p.IdProyecto == dto.ProyectoId);
                if (!existe)
                    throw new ArgumentException("El proyecto especificado no existe.");
            }

            entity.Descripcion = dto.Descripcion;
            entity.FechaInicio = dto.FechaInicio;
            entity.FechaFin = dto.FechaFin;
            entity.RutaDocumento = dto.RutaDocumento;
            entity.ProyectoId = dto.ProyectoId;

            _context.Contratos.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Eliminar contrato laboral
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Contratos.FindAsync(id);
            if (entity == null) return false;

            _context.Contratos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
