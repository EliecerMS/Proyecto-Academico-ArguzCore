using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Domain.Entities;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IProyectoService
    {
        Task CrearProyectoAsync(ProyectoDto Proyecto);
        Task<IEnumerable<ProyectoDto>> MostrarProyectosAsync();
        Task<ProyectoDto> MostrarProyectoPorId(string IdProyecto);
        Task ActualizarProyectoAsync(ProyectoDto Proyecto);
        Task EliminarProyectoAsync(string CodigoProyecto);
        Task<Proyecto?> DetallesProyecto(string CodigoProyecto);
        Task<int> RecalculoPorcentajeAvance(string CodigoProyecto);//metodo para recalcular el porcentaje de avance del proyecto

        //metodo para cambiar el estado de un proyecto
        Task CambiarEstadoAsync(string CodigoProyecto, int IdEstadoProyecto);

        Task<IEnumerable<ProyectoDto>> MostrarProyectosListaReasignacionAsync(string codigoProyecto);
        //metodo para mostrar personal asignado del proyecto
        Task<IEnumerable<PersonalAsignadoDto>> ObtenerPersonalPorProyectoAsync(string codigoProyecto);
        
        
        

        //metodo para asignar nuevo personal a un proyecto
        Task AsignarPersonalAProyectoAsync(string codigoProyecto, string personalId);

        //metodo para reasignar nuevo personal a un proyecto
        Task ReasignarPersonalEnProyectoAsync(string codigoProyecto, string personalId, string codigoProyectoNuevo);

        //metodo para eliminar personal de un proyecto
        Task EliminarPersonalDeProyectoAsync(string codigoProyecto, string personalId);


        // ====================== MÉTODOS PARA TAREAS (Checho)======================
        //metodo para mostrar tareas de un proyecto
        Task<IEnumerable<TareaDto>> MostrarTareasPorProyectoAsync(int IdProyecto);

        //metodo para crear tarea de un proyecto
        Task CrearTareaAsync(TareaDto tarea);

        //metodo para editar tarea de un proyecto
        Task EditarTareaAsync(TareaDto tarea);

        //metodo para eliminar tarea de un proyecto
        Task EliminarTareaAsync(int IdTarea);


        // ====================== MÉTODOS PARA NOTAS (Checho)======================
        //metodo para mostrar notas de avance de un proyecto


        //metodo para crear nota de avance de un proyecto


        //metodo para editar nota de avance de un proyecto


        //metodo para destacar nota de avance de un proyecto





        // Método para listar incidentes de un proyecto
        Task<IEnumerable<Incidente>> MostrarIncidentesPorProyectoAsync(int proyectoId);

        // Método para registrar incidente de un proyecto
        Task CrearIncidenteAsync(Incidente incidente);

        // Método para editar/cerrar (actualizar) un incidente
        Task ActualizarIncidenteAsync(Incidente incidente);

        // Método para obtener un incidente por Id (usado en editar y cerrar)
        Task<Incidente> ObtenerIncidentePorIdAsync(int idIncidente);



        //metodo para mostrar las solicitudes de material del proyecto enviadas a bodega central


        // metotodo para editar la solicitud de material enviada a bodega central


        //metodo para eliminar solicitud de material enviada a bodega central
    }
}
