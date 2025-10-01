using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class ProyectoService : IProyectoService 
    {

        private readonly ApplicationDbContext _context;

        public ProyectoService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task ActualizarProyectoAsync(ProyectoDto Proyecto)
        {
            var ProyectoEncontrado = _context.Proyectos.Include(p => p.EstadoProyecto).FirstOrDefault(p => p.CodigoProyecto == Proyecto.CodigoProyecto);//busca el proyecto por código e incluye la propiedad de navegacion para acceder al codigo estado
            if (ProyectoEncontrado != null) //si lo encuentra actualiza solo los campos editables
            {
                ProyectoEncontrado.Nombre = Proyecto.Nombre;
                ProyectoEncontrado.Descripcion = Proyecto.Descripcion;
                ProyectoEncontrado.FechaFinalPropuesta = Proyecto.FechaFinalPropuesta;
                ProyectoEncontrado.Presupuesto = Proyecto.Presupuesto;

                _context.SaveChanges();
            }
        }

        public async Task CrearProyectoAsync(ProyectoDto Proyecto)
        {

            //crear codigo unico para el proyecto
            string CodigoProyecto = Guid.NewGuid().ToString();

            // Mapear ProyectoDto a Proyecto y asignar el codigo unico, y estado inicial 1 (Planificado)
            var proyecto = new Proyecto { CodigoProyecto = CodigoProyecto, Nombre = Proyecto.Nombre, Descripcion = Proyecto.Descripcion, FechaFinalPropuesta = Proyecto.FechaFinalPropuesta, Presupuesto = Proyecto.Presupuesto, EstadoProyectoId = 1 };
            _context.Proyectos.Add(proyecto);
            await _context.SaveChangesAsync();
        }

        public async Task<Proyecto?> DetallesProyecto(string CodigoProyecto)
        {
            // Usar el método que carga dinámicamente todas las propiedades de navegación
            return await BuscarProyectoPorCodigo(CodigoProyecto);
        }

        public async Task EliminarProyectoAsync(string CodigoProyecto)
        {
            Proyecto? ProyectoEncontrado = await BuscarProyectoPorCodigo(CodigoProyecto);
            if (ProyectoEncontrado != null) //si lo encuentra lo elimina
            {
                _context.Proyectos.Remove(ProyectoEncontrado);
                _context.SaveChanges();
            }
        }

        private async Task<Proyecto?> BuscarProyectoPorCodigo(string CodigoProyecto) //busca el proyecto por codigo (string) y devuelve la entidad Proyecto con todos sus datos
        {
            var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.CodigoProyecto == CodigoProyecto);
            if (proyecto != null)
            {
                var entry = _context.Entry(proyecto);

                // Cargar todas las navegaciones (referencias y colecciones) de forma dinámica
                foreach (var navigation in entry.Navigations)
                {
                    if (!navigation.IsLoaded)
                    {
                        await navigation.LoadAsync();
                    }
                }
            }

            return proyecto;
        }

        public Task<ProyectoDto> MostrarProyectoPorId(string IdProyecto)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ProyectoDto>> MostrarProyectosAsync()
        {
            var proyectos = _context.Proyectos.Include(p => p.EstadoProyecto).ToList();
            var ListaProyectos = new List<ProyectoDto>();

            foreach (var proyecto in proyectos)
            {
                ListaProyectos.Add(new ProyectoDto
                {
                    Descripcion = proyecto.Descripcion,
                    CodigoProyecto = proyecto.CodigoProyecto,
                    Nombre = proyecto.Nombre,
                    FechaFinalPropuesta = proyecto.FechaFinalPropuesta.Date,
                    Presupuesto = proyecto.Presupuesto,
                    EstadoProyecto = proyecto.EstadoProyecto.NombreEstado
                });
            }

            return ListaProyectos;
        }

        //Metodo para cambiar el estado de un proyecto
        public async Task CambiarEstadoAsync(string CodigoProyecto, int IdEstadoProyecto)
        {
            var proyecto = await BuscarProyectoPorCodigo(CodigoProyecto);
            if (proyecto != null)
            {
                proyecto.EstadoProyectoId = IdEstadoProyecto;
                await _context.SaveChangesAsync();
            }
        }

        public async Task AsignarPersonalAProyectoAsync(string codigoProyecto, string personalId)
        {
            // Buscar el proyecto por su código
            var proyecto = await _context.Set<Proyecto>()
                .FirstOrDefaultAsync(p => p.CodigoProyecto == codigoProyecto);

            if (proyecto == null)
                throw new InvalidOperationException("Proyecto no encontrado.");

            // Convertir el personalId a string para comparar con UsuarioId
            //string usuarioId = personalId.ToString();

            // Verificar si el usuario ya está asignado al proyecto
            bool yaAsignado = await _context.PersonalProyecto
                .AnyAsync(pp => pp.ProyectoId == proyecto.IdProyecto && pp.UsuarioId == personalId);

            if (yaAsignado)
                throw new InvalidOperationException("El personal ya está asignado a este proyecto.");

            // Crear el registro de asignación
            var asignacion = new PersonalProyecto
            {
                ProyectoId = proyecto.IdProyecto,
                UsuarioId = personalId
            };

            _context.PersonalProyecto.Add(asignacion);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarPersonalDeProyectoAsync(string codigoProyecto, string personalId)
        {
            // Buscar el proyecto por su código
            var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.CodigoProyecto == codigoProyecto);
            if (proyecto == null)
                throw new InvalidOperationException("Proyecto no encontrado.");

            // Buscar la asignación de personal en el proyecto
            var asignacion = await _context.PersonalProyecto
                .FirstOrDefaultAsync(pp => pp.ProyectoId == proyecto.IdProyecto && pp.UsuarioId == personalId);

            if (asignacion == null)
                throw new InvalidOperationException("El personal no está asignado a este proyecto.");

            _context.PersonalProyecto.Remove(asignacion);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PersonalAsignadoDto>> ObtenerPersonalPorProyectoAsync(string codigoProyecto)
        {
            // Buscar el proyecto por su código
            var proyecto = await _context.Set<Proyecto>()
                .FirstOrDefaultAsync(p => p.CodigoProyecto == codigoProyecto);

            if (proyecto == null)
                return Enumerable.Empty<PersonalAsignadoDto>();

            // Obtener los registros de personal asignado al proyecto
            var personalProyectos = await _context.PersonalProyecto
                .Where(pp => pp.ProyectoId == proyecto.IdProyecto)
                .ToListAsync();

            // Obtener los datos de usuario para cada personal asignado
            var usuariosIds = personalProyectos.Select(pp => pp.UsuarioId).ToList();

            var usuarios = await _context.Users
                .Where(u => usuariosIds.Contains(u.Id))
                .ToListAsync();

            // Mapear a DTO
            var resultado = personalProyectos
                .Select(pp =>
                {
                    var usuario = usuarios.FirstOrDefault(u => u.Id == pp.UsuarioId);
                    return new PersonalAsignadoDto
                    {
                        PersonalId = usuario?.Id ?? "Desconocido",
                        Nombre = usuario?.NombreCompleto ?? "Desconocido"
                    };
                })
                .ToList();

            return resultado;
        }

        public async Task ReasignarPersonalEnProyectoAsync(string codigoProyecto, string personalId, string codigoProyectoNuevo)
        {
            // Buscar el proyecto actual
            var proyectoActual = await _context.Proyectos.FirstOrDefaultAsync(p => p.CodigoProyecto == codigoProyecto);
            if (proyectoActual == null)
                throw new InvalidOperationException("Proyecto actual no encontrado.");

            // Buscar el nuevo proyecto
            var proyectoNuevo = await _context.Proyectos.FirstOrDefaultAsync(p => p.CodigoProyecto == codigoProyectoNuevo);
            if (proyectoNuevo == null)
                throw new InvalidOperationException("Proyecto nuevo no encontrado.");

            // Buscar la asignación actual del personal en el proyecto actual
            var asignacionActual = await _context.PersonalProyecto
                .FirstOrDefaultAsync(pp => pp.ProyectoId == proyectoActual.IdProyecto && pp.UsuarioId == personalId);

            if (asignacionActual == null)
                throw new InvalidOperationException("El personal no está asignado al proyecto actual.");

            // Verificar si ya está asignado al nuevo proyecto
            var yaAsignadoNuevo = await _context.PersonalProyecto
                .AnyAsync(pp => pp.ProyectoId == proyectoNuevo.IdProyecto && pp.UsuarioId == personalId);

            if (yaAsignadoNuevo)
                throw new InvalidOperationException("El personal ya está asignado al nuevo proyecto.");

            // Eliminar la asignación del proyecto actual
            _context.PersonalProyecto.Remove(asignacionActual);

            // Crear la nueva asignación en el nuevo proyecto
            var nuevaAsignacion = new PersonalProyecto
            {
                ProyectoId = proyectoNuevo.IdProyecto,
                UsuarioId = personalId
            };
            _context.PersonalProyecto.Add(nuevaAsignacion);

            await _context.SaveChangesAsync();
        }
    }
}

