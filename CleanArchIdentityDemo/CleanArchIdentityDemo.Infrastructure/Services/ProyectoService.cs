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

        public async Task EliminarProyectoAsync(string CodigoProyecto)
        {
            var ProyectoEncontrado = await _context.Proyectos.FirstOrDefaultAsync(p => p.CodigoProyecto == CodigoProyecto);//busca el proyecto por codigo (string)
            if (ProyectoEncontrado != null) //si lo encuentra lo elimina
            {
                _context.Proyectos.Remove(ProyectoEncontrado);
                _context.SaveChanges();
            }
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
    }
}
