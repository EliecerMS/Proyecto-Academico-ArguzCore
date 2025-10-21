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

        public EquipoService(
            ApplicationDbContext context)
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

        // Eliminar un equipo por ID y a su vez eliminar sus relaciones
        public async Task<bool> DeleteAsync(int id)
        {
            // Traemos la maquinaria
            var maquinaria = await _context.Maquinarias.FindAsync(id);
            if (maquinaria == null) return false;

            // Eliminamos el proyecto asignado (si existe)
            var proyecto = await _context.MaquinariaProyecto
                .FirstOrDefaultAsync(mp => mp.MaquinariaId == id);
            if (proyecto != null)
            {
                _context.MaquinariaProyecto.Remove(proyecto);
            }

            // Eliminamos todos los mantenimientos relacionados
            var mantenimientos = await _context.MantenimientosMaquinaria
                .Where(m => m.MaquinariaId == id)
                .ToListAsync();
            if (mantenimientos.Any())
            {
                _context.MantenimientosMaquinaria.RemoveRange(mantenimientos);
            }

            // Finalmente, eliminamos la maquinaria
            _context.Maquinarias.Remove(maquinaria);

            await _context.SaveChangesAsync();
            return true;
        }



        // Asignar un proyecto a una maquinaria
        public async Task<bool> AsignarProyectoAsync(int idMaquinaria, int idProyecto)
        {
            var maquinaria = await _context.Maquinarias.FindAsync(idMaquinaria); // Confirmar la maquinaria
            if (maquinaria == null) return false; // Evitar errores

            var proyectoExistente = await _context.MaquinariaProyecto // Confirmar que no tenga proyecto asignado
                .FirstOrDefaultAsync(mp => mp.MaquinariaId == idMaquinaria);
            if (proyectoExistente != null) return false; //Evitar asignar más de un proyecto a la vez

            var proyecto = await _context.Proyectos.FindAsync(idProyecto); // Trae el proyecto para extrae el nombre/Ubicación
            if (proyecto == null) return false; // Evitar errores

            maquinaria.Estado = "En Servicio"; // La maquinaria pasa a estar en servicio
            maquinaria.Ubicacion = proyecto.Nombre; // La ubicación se actualiza al nombre del proyecto asignado

            var nuevoProyecto = new MaquinariaProyecto //Se genera el nuevo registro de asignación
            {
                MaquinariaId = idMaquinaria,
                ProyectoId = idProyecto,
                FechaAsignacion = DateTime.Now
            };

            _context.MaquinariaProyecto.Add(nuevoProyecto); // Se añade la nueva asignación
            await _context.SaveChangesAsync();
            return true;
        }

        // Desasignar un proyecto de una maquinaria
        public async Task<bool> DesasignarProyectoAsync(int idMaquinaria)
        {
            var proyecto = await _context.MaquinariaProyecto
                .FirstOrDefaultAsync(mp => mp.MaquinariaId == idMaquinaria);
            if (proyecto == null) return false;

            var maquinaria = await _context.Maquinarias.FindAsync(idMaquinaria);
            if (maquinaria != null)
            {
                maquinaria.Estado = "Fuera de Servicio";
                maquinaria.Ubicacion = "Bodega Central";
            }

            _context.MaquinariaProyecto.Remove(proyecto); // Se borra el registro porque la maquianria ya no está asignada a ningún proyecto, cabe destacar que esto se puede mantener pero hay que añadir otra propiedad al entidad para marcar si está activa o no la asignación
            await _context.SaveChangesAsync();
            return true;
        }

        // Devuelve el proyecto al que está asociada una maquinaria
        public async Task<MaquinariaProyectoDto?> GetProyectoAsignadoAsync(int idMaquinaria)
        {
            var proyecto = await _context.MaquinariaProyecto
                .Include(mp => mp.Proyecto)
                .FirstOrDefaultAsync(mp => mp.MaquinariaId == idMaquinaria);

            if (proyecto == null) return null;

            return new MaquinariaProyectoDto
            {
                IdMaquinariaProyecto = proyecto.IdMaquinariaProyecto,
                MaquinariaId = proyecto.MaquinariaId,
                ProyectoId = proyecto.ProyectoId,
                FechaAsignacion = proyecto.FechaAsignacion,
            };
        }


        // Iniciar mantenimiento de una maquinaria
        public async Task<bool> IniciarMantenimientoAsync(int idMaquinaria)
        {
            var maquinaria = await _context.Maquinarias //Traemos maquinaria
                .Include(m => m.ProyectosAsignados) // Junto a su registro de proyecto asignado
                .FirstOrDefaultAsync(m => m.IdMaquinaria == idMaquinaria);

            if (maquinaria == null) return false; // Confirmamos que exista para evitar errores

            var proyectoActivo = maquinaria.ProyectosAsignados.FirstOrDefault(); // Verificamos si tiene un proyecto asignado
            if (proyectoActivo != null) // En caso de que tenga
            {
                maquinaria.Estado = "Mantenimiento";
                maquinaria.Ubicacion = "Bodega Central";

                _context.MaquinariaProyecto.Remove(proyectoActivo); 
                // NOTA: Esto se mantiene en caso de no modificar la BD, si se añade un parametro extra a MaquinariaProyecto, se podría mantener un historial de asignaciones de proyectos
            }
            else // En caso de que no tenga
            {
                maquinaria.Estado = "Mantenimiento";
                maquinaria.Ubicacion = "Bodega Central";
            }

            var mantenimiento = new MantenimientoMaquinaria // Crear registro de mantenimiento
            {
                MaquinariaId = idMaquinaria,
                FechaProgramada = DateTime.Now,
                Estado = "Ejecución",
                FechaCompletado = DateTime.MinValue // Se debe hacer modificación en la BD para cambiar esto a NULLABLE
            };

            _context.MantenimientosMaquinaria.Add(mantenimiento);
            await _context.SaveChangesAsync();
            return true;
        }

        // Finalizar mantenimiento
        public async Task<bool> FinalizarMantenimientoAsync(int idMaquinaria)
        {
            var mantenimiento = await _context.MantenimientosMaquinaria
                .Where(m => m.MaquinariaId == idMaquinaria && m.Estado == "Ejecución")
                .FirstOrDefaultAsync(); // Llamamos específicamente al registro de mantenimiento que pertenezca a la maquinaria dada y que esté activo.

            if (mantenimiento == null) return false; // Para errores

            mantenimiento.Estado = "Finalizado"; // Se finaliza el mantenimiento
            mantenimiento.FechaCompletado = DateTime.Now;

            var maquinaria = await _context.Maquinarias.FindAsync(idMaquinaria); // Se trae a la maquinaria que dejó al mantenimiento
            if (maquinaria != null)
            {
                maquinaria.Estado = "Fuera de Servicio"; 
                maquinaria.Ubicacion = "Bodega Central";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Devuelve el historial de manteniminetos de una maquinaria
        public async Task<IEnumerable<MantenimientoMaquinariaDto>> GetMantenimientosPorMaquinariaAsync(int idMaquinaria) //Para listar el historico de manteniminetos
        {
            return await _context.MantenimientosMaquinaria
                .Where(m => m.MaquinariaId == idMaquinaria)
                .Select(m => new MantenimientoMaquinariaDto
                {
                    IdMantenimiento = m.IdMantenimiento,
                    MaquinariaId = m.MaquinariaId,
                    Estado = m.Estado,
                    FechaProgramada = m.FechaProgramada,
                    FechaCompletado = m.FechaCompletado
                })
                .ToListAsync();
        }

    }
}
