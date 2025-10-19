using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class EquipoService : IEquipoService
    {
        //implementacion de los metodos para realizar operaciones en la vista de Equipo
        private readonly ApplicationDbContext _context;

        public EquipoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener todos los equipos
        public async Task<IEnumerable<MaquinariaDto>> GetAllAsync()
        {
            var equipos = await _context.Maquinarias
                .Select(m => new MaquinariaDto
                {
                    IdMaquinaria = m.IdMaquinaria,
                    CodigoMaquinaria = m.CodigoMaquinaria,
                    Nombre = m.Nombre,
                    Tipo = m.Tipo,
                    Descripcion = m.Descripcion,
                    NumeroSerie = m.NumeroSerie,
                    Estado = m.Estado,
                    Ubicacion = m.Ubicacion
                })
                .ToListAsync();

            return equipos;
        }

        // Obtener un equipo por ID
        public async Task<MaquinariaDto?> GetByIdAsync(int id)
        {
            var m = await _context.Maquinarias.FindAsync(id);
            if (m == null) return null;

            return new MaquinariaDto
            {
                IdMaquinaria = m.IdMaquinaria,
                CodigoMaquinaria = m.CodigoMaquinaria,
                Nombre = m.Nombre,
                Tipo = m.Tipo,
                Descripcion = m.Descripcion,
                NumeroSerie = m.NumeroSerie,
                Estado = m.Estado,
                Ubicacion = m.Ubicacion
            };
        }

        // Crear un nuevo equipo
        public async Task<MaquinariaDto> CreateAsync(MaquinariaDto dto)
        {
            var entity = new Maquinaria
            {
                CodigoMaquinaria = Guid.NewGuid().ToString(), // Código único
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                NumeroSerie = dto.NumeroSerie,
                Estado = "Fuera de Servicio",  // Default al crear ya que no está asignado a ningún proyecto
                Ubicacion = string.Empty       // Sin proyecto asignado al crear ya que no está asignado a ningún proyecto
            };

            _context.Maquinarias.Add(entity);
            await _context.SaveChangesAsync();

            dto.IdMaquinaria = entity.IdMaquinaria;
            dto.Estado = entity.Estado;
            dto.Ubicacion = entity.Ubicacion;
            dto.CodigoMaquinaria = entity.CodigoMaquinaria;

            return dto;
        }


        // Actualizar un equipo existente
        public async Task<bool> UpdateAsync(MaquinariaDto dto)
        {
            var entity = await _context.Maquinarias.FindAsync(dto.IdMaquinaria);
            if (entity == null) return false;

            // Solo se permite editar nombre, tipo, descripción y número de serie ya que el resto se hará a através de asignaciones a proyectos y mantenimientos
            entity.Nombre = dto.Nombre;
            entity.Tipo = dto.Tipo;
            entity.Descripcion = dto.Descripcion;
            entity.NumeroSerie = dto.NumeroSerie;

            _context.Maquinarias.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }


        // Eliminar un equipo por ID
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Maquinarias.FindAsync(id);
            if (entity == null) return false;

            _context.Maquinarias.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> AsignarProyectoAsync(int idMaquinaria, string nombreProyecto)
        {
            throw new NotImplementedException();
        }
    }
}
