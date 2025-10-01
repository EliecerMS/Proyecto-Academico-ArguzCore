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
                var PorcentajeAvance = await RecalculoPorcentajeAvance(proyecto.CodigoProyecto); // recalcula el porcentaje de avance antes de mostrar la lista
                ListaProyectos.Add(new ProyectoDto
                {
                    Descripcion = proyecto.Descripcion,
                    CodigoProyecto = proyecto.CodigoProyecto,
                    Nombre = proyecto.Nombre,
                    FechaFinalPropuesta = proyecto.FechaFinalPropuesta.Date,
                    Presupuesto = proyecto.Presupuesto,
                    EstadoProyecto = proyecto.EstadoProyecto.NombreEstado,
                    PorcentajeAvance = PorcentajeAvance
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


        public async Task<int> RecalculoPorcentajeAvance(string CodigoProyecto)
        {
            Proyecto? ProyectoEncontrado = await BuscarProyectoPorCodigo(CodigoProyecto);
            if (ProyectoEncontrado == null) return 0;

            var tareas = ProyectoEncontrado.Tareas;
            if (tareas == null || tareas.Count == 0) return 0;

            var ahora = DateTime.Now;
            double sumaPorcentajes = 0;

            foreach (var tarea in tareas)
            {
                // Tarea aún no ha iniciado
                if (ahora < tarea.FechaInicioEsperada)
                {
                    sumaPorcentajes += 0;
                }
                // Tarea ya debería estar completada
                else if (ahora >= tarea.FechaFinalEsperada)
                {
                    sumaPorcentajes += 100;
                }
                // Tarea en progreso - calcular porcentaje proporcional
                else
                {
                    var duracionTarea = (tarea.FechaFinalEsperada - tarea.FechaInicioEsperada).TotalSeconds;
                    var transcurridoTarea = (ahora - tarea.FechaInicioEsperada).TotalSeconds;

                    if (duracionTarea > 0)
                    {
                        double porcentajeTarea = (transcurridoTarea / duracionTarea) * 100.0;
                        sumaPorcentajes += Math.Clamp(porcentajeTarea, 0, 100);
                    }
                }
            }

            // Promedio de todas las tareas
            int porcentajeProyecto = (int)Math.Round(sumaPorcentajes / tareas.Count);
            return Math.Clamp(porcentajeProyecto, 0, 100);

        }
    }
}
