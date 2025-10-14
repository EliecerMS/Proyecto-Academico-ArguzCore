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

        // ====================== MÉTODOS PARA TAREAS (Checho)======================
        public async Task<IEnumerable<TareaDto>> MostrarTareasPorProyectoAsync(int IdProyecto)
        {
            var proyecto = await _context.Proyectos // Se llama la tabla Proyectos
                .Include(p => p.Tareas) // incluir las tareas asociadas
                .FirstOrDefaultAsync(p => p.IdProyecto == IdProyecto); // buscar el proyecto por su Id

            if (proyecto == null) return new List<TareaDto>(); // Si no se encuentra el proyecto, devolver una lista vacía

            return proyecto.Tareas.Select(t => new TareaDto // Mapear cada tarea a TareaDto
            {
                Id = t.IdTarea,
                Nombre = t.NombreTarea,
                Descripcion = t.Descripcion,
                FechaInicio = t.FechaInicioEsperada,
                FechaFin = t.FechaFinalEsperada,
                ProyectoId = proyecto.IdProyecto
            }).ToList(); // Devolver la lista de TareaDto
        }

        public async Task CrearTareaAsync(TareaDto dto)
        {
            var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.IdProyecto == dto.ProyectoId); // buscar el proyecto por su Id

            if (proyecto == null) throw new Exception("Proyecto no encontrado"); // Si no se encuentra el proyecto, lanzar una excepción

            var nuevaTarea = new Tarea // Crear una nueva instancia de Tarea
            {
                NombreTarea = dto.Nombre,
                Descripcion = dto.Descripcion,
                FechaInicioEsperada = dto.FechaInicio,
                FechaFinalEsperada = dto.FechaFin,
                ProyectoId = proyecto.IdProyecto
            };

            _context.Tareas.Add(nuevaTarea); // Agregar la nueva tarea al contexto
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos
        }

        public async Task EditarTareaAsync(TareaDto dto)
        {
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.IdTarea == dto.Id); // buscar la tarea por su Id

            if (tarea != null) //si lo encuentra actualiza solo los campos editables
            { // Actualizar los campos editables
                tarea.NombreTarea = dto.Nombre;
                tarea.Descripcion = dto.Descripcion;
                tarea.FechaInicioEsperada = dto.FechaInicio;
                tarea.FechaFinalEsperada = dto.FechaFin;

                await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos
            }
        }

        public async Task EliminarTareaAsync(int IdTarea)
        {
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.IdTarea == IdTarea); // buscar la tarea por su Id

            if (tarea != null) //si lo encuentra lo elimina
            {
                _context.Tareas.Remove(tarea); // Eliminar la tarea del contexto
                await _context.SaveChangesAsync();// Guardar los cambios en la base de datos
            }
        }

        // ====================== MÉTODOS PARA PERSONAL DEL PROYECTO======================
        public async Task AsignarPersonalAProyectoAsync(string codigoProyecto, string personalId)
        {
            // Buscar el proyecto por su código
            var proyecto = await _context.Set<Proyecto>()
                .FirstOrDefaultAsync(p => p.CodigoProyecto == codigoProyecto);
            if (proyecto == null)
                throw new InvalidOperationException("Proyecto no encontrado.");

            // Verificar si el usuario ya está asignado al proyecto
            bool yaAsignadoEste = await _context.PersonalProyecto
                .AnyAsync(pp => pp.ProyectoId == proyecto.IdProyecto && pp.UsuarioId == personalId);

            if (yaAsignadoEste)
                throw new InvalidOperationException("El personal ya está asignado a este proyecto.");

            // Ya está en otro proyecto
            bool yaAsignadoOtro = await _context.PersonalProyecto
                .AnyAsync(pp => pp.UsuarioId == personalId && pp.ProyectoId != proyecto.IdProyecto);

            if (yaAsignadoOtro)
                throw new InvalidOperationException("El personal ya está asignado a otro proyecto.");

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

        public async Task<IEnumerable<ProyectoDto>> MostrarProyectosListaReasignacionAsync(string codigoProyectoActual)
        {
            var proyectos = await _context.Proyectos
                .Include(p => p.EstadoProyecto)
                .Where(p => p.CodigoProyecto != codigoProyectoActual)
                .ToListAsync();

            var listaProyectos = new List<ProyectoDto>();

            foreach (var proyecto in proyectos)
            {
                var porcentajeAvance = await RecalculoPorcentajeAvance(proyecto.CodigoProyecto);
                listaProyectos.Add(new ProyectoDto
                {
                    Descripcion = proyecto.Descripcion,
                    CodigoProyecto = proyecto.CodigoProyecto,
                    Nombre = proyecto.Nombre,
                    FechaFinalPropuesta = proyecto.FechaFinalPropuesta.Date,
                    Presupuesto = proyecto.Presupuesto,
                    EstadoProyecto = proyecto.EstadoProyecto?.NombreEstado,
                    PorcentajeAvance = porcentajeAvance
                });
            }

            return listaProyectos;
        }
        //Materiales

        //Crear solicitud de Material
        public async Task CrearSolicitudMaterialAsync(SolicitudMaterialDto solicitudDto)
        {
            var solicitud = new SolicitudMaterial
            {
                ProyectoId = solicitudDto.ProyectoId,
                FechaSolicitud = solicitudDto.FechaSolicitud,
                EstadoSolicitud = solicitudDto.EstadoSolicitud,
                ObservacionesBodeguero = "Sin observaciones aún",
                MaterialesSolicitados = solicitudDto.MaterialesSolicitados.Select(ms => new MaterialSolicitado
                {
                    MaterialId = ms.MaterialId,
                    Cantidad = ms.Cantidad,
                    Prioridad = ms.Prioridad
                }).ToList()
            };

            _context.SolicitudesMaterial.Add(solicitud);
            await _context.SaveChangesAsync();
        }


        //Obtener solicitud por ID
        public async Task<SolicitudMaterialDto?> ObtenerSolicitudPorIdAsync(int idSolicitud)
        {
            var solicitud = await _context.SolicitudesMaterial
                .Include(s => s.MaterialesSolicitados)
                .ThenInclude(ms => ms.Material)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null)
                return null;

            return new SolicitudMaterialDto
            {
                IdSolicitud = solicitud.IdSolicitud,
                ProyectoId = solicitud.ProyectoId,
                FechaSolicitud = solicitud.FechaSolicitud,
                EstadoSolicitud = solicitud.EstadoSolicitud,
                ObservacionesBodeguero = solicitud.ObservacionesBodeguero,
                MaterialesSolicitados = solicitud.MaterialesSolicitados.Select(ms => new MaterialSolicitadoDto
                {
                    IdMaterialSolicitado = ms.IdMaterialSolicitado,
                    MaterialId = ms.MaterialId,
                    NombreMaterial = ms.Material?.NombreMaterial ?? "",
                    Cantidad = ms.Cantidad,
                    Prioridad = ms.Prioridad
                }).ToList()
            };
        }

        //Actualizar Solicitud Material
        public async Task ActualizarSolicitudAsync(SolicitudMaterialDto solicitudDto)
        {
            var solicitud = await _context.SolicitudesMaterial
                .Include(s => s.MaterialesSolicitados)
                .FirstOrDefaultAsync(s => s.IdSolicitud == solicitudDto.IdSolicitud);

            if (solicitud != null)
            {
                solicitud.ObservacionesBodeguero = solicitudDto.ObservacionesBodeguero;
                solicitud.EstadoSolicitud = solicitudDto.EstadoSolicitud;

                var materialSolicitado = solicitud.MaterialesSolicitados.FirstOrDefault();
                if (materialSolicitado != null && solicitudDto.MaterialesSolicitados.Any())
                {
                    var dtoMat = solicitudDto.MaterialesSolicitados.First();
                    materialSolicitado.MaterialId = dtoMat.MaterialId;
                    materialSolicitado.Cantidad = dtoMat.Cantidad;
                    materialSolicitado.Prioridad = dtoMat.Prioridad;
                }

                await _context.SaveChangesAsync();
            }
        }

        //Obtener Materiales 
        public async Task<IEnumerable<MaterialDto>> ObtenerMaterialesAsync()
        {
            return await _context.Materiales
                .Select(m => new MaterialDto
                {
                    IdMaterial = m.IdMaterial,
                    NombreMaterial = m.NombreMaterial,
                    Tipo = m.Tipo,
                    Descripcion = m.Descripcion,
                    CantidadDisponible = m.CantidadDisponible
                })
                .ToListAsync();
        }


        //Mostrar solicitud por proyecto
        public async Task<IEnumerable<SolicitudMaterialDto>> MostrarSolicitudesPorProyectoAsync(int proyectoId)
        {
            var solicitudes = await _context.SolicitudesMaterial
                .Include(s => s.MaterialesSolicitados)
                .ThenInclude(ms => ms.Material)
                .Where(s => s.ProyectoId == proyectoId)
                .ToListAsync();

            return solicitudes.Select(s => new SolicitudMaterialDto
            {
                IdSolicitud = s.IdSolicitud,
                ProyectoId = s.ProyectoId,
                FechaSolicitud = s.FechaSolicitud,
                EstadoSolicitud = s.EstadoSolicitud,
                ObservacionesBodeguero = s.ObservacionesBodeguero,
                MaterialesSolicitados = s.MaterialesSolicitados.Select(ms => new MaterialSolicitadoDto
                {
                    IdMaterialSolicitado = ms.IdMaterialSolicitado,
                    MaterialId = ms.MaterialId,
                    NombreMaterial = ms.Material?.NombreMaterial ?? "",
                    Cantidad = ms.Cantidad,
                    Prioridad = ms.Prioridad
                }).ToList()
            }).ToList();
        }

        //Elimar solicitud proyecto
        public async Task EliminarSolicitudMaterialAsync(int idSolicitud)
        {
            var solicitud = await _context.SolicitudesMaterial
                .Include(s => s.MaterialesSolicitados)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud != null)
            {
                _context.MaterialesSolicitados.RemoveRange(solicitud.MaterialesSolicitados);
                _context.SolicitudesMaterial.Remove(solicitud);
                await _context.SaveChangesAsync();
            }
        }






        // ====================== MÉTODOS PARA NOTAS (Checho)======================
        public async Task<List<NotaAvanceDto>> MostrarNotasAsync(int idProyecto)
        {
            return await _context.NotasAvance
                .Where(n => n.ProyectoId == idProyecto)
                .OrderByDescending(n => n.FechaNota)
                .Select(n => new NotaAvanceDto
                {
                    IdNota = n.IdNota,
                    ProyectoId = n.ProyectoId,
                    Descripcion = n.Descripcion,
                    Destacada = n.Destacada,
                    FechaNota = n.FechaNota,
                    CreadoPor = n.CreadoPor
                })
                .ToListAsync();
        }

        public async Task CrearNotaAsync(NotaAvanceDto notaDto)
        {
            var nota = new NotaAvance
            {
                ProyectoId = notaDto.ProyectoId,
                Descripcion = notaDto.Descripcion,
                Destacada = notaDto.Destacada,
                FechaNota = notaDto.FechaNota,
                CreadoPor = notaDto.CreadoPor
            };

            _context.NotasAvance.Add(nota);
            await _context.SaveChangesAsync();
        }

        public async Task EditarNotaAsync(NotaAvanceDto notaDto)
        {
            var notaExistente = await _context.NotasAvance
                .FirstOrDefaultAsync(n => n.IdNota == notaDto.IdNota);

            notaExistente.Descripcion = notaDto.Descripcion;
            notaExistente.FechaNota = notaDto.FechaNota;
            notaExistente.CreadoPor = notaDto.CreadoPor;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarNotaAsync(int IdNota)
        {
            var nota = await _context.NotasAvance.FirstOrDefaultAsync(n => n.IdNota == IdNota);

            if (nota != null) //si lo encuentra lo elimina
            {
                _context.NotasAvance.Remove(nota); // Eliminar la tarea del contexto
                await _context.SaveChangesAsync();// Guardar los cambios en la base de datos
            }
        }

        public async Task<bool> DestacarNotaAsync(int idNota)
        {
            var nota = await _context.NotasAvance
                .FirstOrDefaultAsync(n => n.IdNota == idNota);

            if (nota == null)
            {
                return false;
            }

            nota.Destacada = !nota.Destacada;
            await _context.SaveChangesAsync();
            return true;
        }




        //Mostrar incidente 
        public async Task<IEnumerable<Incidente>> MostrarIncidentesPorProyectoAsync(int proyectoId)
        {
            return await _context.Incidentes
                .Where(i => i.ProyectoId == proyectoId)
                .OrderByDescending(i => i.FechaRegistro)
                .ToListAsync();
        }

        //Crear Incidente
        public async Task CrearIncidenteAsync(Incidente incidente)
        {
            _context.Incidentes.Add(incidente);
            await _context.SaveChangesAsync();
        }

        //Cerrar Incidente
        public async Task<Incidente> ObtenerIncidentePorIdAsync(int idIncidente)
        {
            return await _context.Incidentes.FindAsync(idIncidente);
        }

        public async Task ActualizarIncidenteAsync(Incidente incidente)
        {
            _context.Incidentes.Update(incidente);
            await _context.SaveChangesAsync();

        }

    }
}









