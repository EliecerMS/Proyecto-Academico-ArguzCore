using CleanArchIdentityDemo.Application.DTOs;

namespace CleanArchIdentityDemo.Application.Interfaces
{
    public interface IDocumentosService
    {
        // CASO 1: Crear documento versionado
        Task<int> CrearDocumentoVersionadoAsync(CrearDocumentoVersionadoDto dto);

        // CASO 2: Agregar nueva versión
        Task<int> AgregarVersionAsync(AgregarVersionDto dto);

        // CASO 3: Crear documento simple
        Task<int> CrearDocumentoSimpleAsync(CrearDocumentoSimpleDto dto);

        // Consultas
        Task<IEnumerable<DocumentoDto>> ObtenerDocumentosPorProyectoAsync(int proyectoId);
        Task<DocumentoDto?> ObtenerDocumentoPorIdAsync(int idDocumento);
        Task<IEnumerable<DocumentoVersionDto>> ObtenerVersionesPorDocumentoAsync(int documentoId);
        Task<DocumentoVersionDto?> ObtenerVersionPorIdAsync(int idVersion);
        Task<DocumentoVersionDto?> ObtenerVersionActualAsync(int documentoId);

        // Operaciones
        Task MarcarVersionComoActualAsync(int idVersion);
        Task EliminarDocumentoAsync(int idDocumento);
        Task EliminarVersionAsync(int idVersion);
        Task<decimal> ObtenerSiguienteNumeroVersionAsync(int documentoId);
    }
}
