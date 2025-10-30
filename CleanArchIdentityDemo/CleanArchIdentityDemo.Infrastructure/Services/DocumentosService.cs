using CleanArchIdentityDemo.Application.DTOs;
using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using CleanArchIdentityDemo.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchIdentityDemo.Infrastructure.Services
{
    public class DocumentosService : IDocumentosService
    {
        private readonly ApplicationDbContext _context;

        public DocumentosService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // CASO 1: CREAR DOCUMENTO VERSIONADO
        // ==========================================
        public async Task<int> CrearDocumentoVersionadoAsync(CrearDocumentoVersionadoDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var documento = new Documento
                {
                    ProyectoId = dto.ProyectoId,
                    NombreDocumento = dto.NombreDocumento,
                    CategoriaDocumento = dto.CategoriaDocumento ?? string.Empty,
                    Descripcion = dto.Descripcion,
                    TipoDocumento = "Versionado",
                    VersionActual = 1.0m,
                    TotalVersiones = 1,
                    SubidoPor = dto.CreadoPor,
                    FechaSubida = DateTime.Now,
                    UltimaModificacion = DateTime.Now,
                    Activo = true
                };

                _context.Documentos.Add(documento);
                await _context.SaveChangesAsync();

                var primeraVersion = new DocumentoVersion
                {
                    DocumentoId = documento.IdDocumento,
                    NumeroVersion = 1.0m,
                    EsVersionActual = true,
                    NombreArchivoOriginal = dto.NombreArchivoOriginal,
                    NombreBlob = ExtraerNombreBlob(dto.RutaBlobCompleta),
                    RutaBlobCompleta = dto.RutaBlobCompleta,
                    TipoArchivo = dto.TipoArchivo,
                    TamanoBytes = dto.TamanoBytes,
                    SubidoPor = dto.CreadoPor,
                    FechaSubida = DateTime.Now,
                    Comentarios = dto.ComentariosVersion ?? "Versión inicial",
                    Activo = true
                };

                _context.DocumentoVersiones.Add(primeraVersion);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return documento.IdDocumento;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // CASO 2: AGREGAR NUEVA VERSIÓN
        // ==========================================
        public async Task<int> AgregarVersionAsync(AgregarVersionDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var documento = await _context.Documentos
                    .FirstOrDefaultAsync(d => d.IdDocumento == dto.DocumentoId && d.TipoDocumento == "Versionado");

                if (documento == null)
                    throw new InvalidOperationException("Documento no encontrado o no es versionable.");

                var versionesAnteriores = await _context.DocumentoVersiones
                    .Where(v => v.DocumentoId == dto.DocumentoId && v.EsVersionActual)
                    .ToListAsync();

                foreach (var version in versionesAnteriores)
                {
                    version.EsVersionActual = false;
                }

                var nuevoNumeroVersion = await ObtenerSiguienteNumeroVersionAsync(dto.DocumentoId);

                var nuevaVersion = new DocumentoVersion
                {
                    DocumentoId = dto.DocumentoId,
                    NumeroVersion = nuevoNumeroVersion,
                    EsVersionActual = true,
                    NombreArchivoOriginal = dto.NombreArchivoOriginal,
                    NombreBlob = ExtraerNombreBlob(dto.RutaBlobCompleta),
                    RutaBlobCompleta = dto.RutaBlobCompleta,
                    TipoArchivo = dto.TipoArchivo,
                    TamanoBytes = dto.TamanoBytes,
                    SubidoPor = dto.SubidoPor,
                    FechaSubida = DateTime.Now,
                    Comentarios = dto.Comentarios,
                    Activo = true
                };

                _context.DocumentoVersiones.Add(nuevaVersion);

                documento.VersionActual = nuevoNumeroVersion;
                documento.TotalVersiones = await _context.DocumentoVersiones
                    .CountAsync(v => v.DocumentoId == dto.DocumentoId && v.Activo);
                documento.UltimaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return nuevaVersion.IdVersion;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // CASO 3: CREAR DOCUMENTO SIMPLE
        // ==========================================
        public async Task<int> CrearDocumentoSimpleAsync(CrearDocumentoSimpleDto dto)
        {
            var documento = new Documento
            {
                ProyectoId = dto.ProyectoId,
                NombreDocumento = dto.NombreDocumento,
                CategoriaDocumento = dto.CategoriaDocumento ?? string.Empty,
                Descripcion = dto.Descripcion,
                TipoDocumento = "Simple",
                NombreArchivoOriginal = dto.NombreArchivoOriginal,
                NombreBlob = ExtraerNombreBlob(dto.RutaBlobCompleta),
                RutaBlobCompleta = dto.RutaBlobCompleta,
                TipoArchivo = dto.TipoArchivo,
                TamanoBytes = dto.TamanoBytes,
                SubidoPor = dto.CreadoPor,
                FechaSubida = DateTime.Now,
                UltimaModificacion = DateTime.Now,
                Activo = true
            };

            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            return documento.IdDocumento;
        }



        // ==========================================
        // MÉTODO HELPER
        // ==========================================
        private async Task<Dictionary<string, string>> ObtenerNombresUsuariosAsync(IEnumerable<string> usuarioIds)
        {
            var ids = usuarioIds.Distinct().ToList();

            if (!ids.Any())
                return new Dictionary<string, string>();

            var usuarios = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.NombreCompleto })
                .ToListAsync();

            return usuarios.ToDictionary(u => u.Id, u => u.NombreCompleto ?? "Usuario");
        }

        // ==========================================
        // CONSULTAS
        // ==========================================

        public async Task<IEnumerable<DocumentoDto>> ObtenerDocumentosPorProyectoAsync(int proyectoId)
        {
            // 1. Obtener documentos
            var documentos = await _context.Documentos
                .Where(d => d.ProyectoId == proyectoId && d.Activo)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            if (!documentos.Any())
                return new List<DocumentoDto>();

            // 2. Obtener versiones
            var documentoIds = documentos.Select(d => d.IdDocumento).ToList();
            var versiones = await _context.DocumentoVersiones
                .Where(v => documentoIds.Contains(v.DocumentoId) && v.Activo)
                .OrderByDescending(v => v.NumeroVersion)
                .ToListAsync();

            // 3. Obtener todos los usuarios necesarios
            var todosUsuarioIds = documentos.Select(d => d.SubidoPor)
                .Concat(versiones.Select(v => v.SubidoPor))
                .ToList();

            var nombresUsuarios = await ObtenerNombresUsuariosAsync(todosUsuarioIds);

            // 4. Mapear a DTOs
            return documentos.Select(d => new DocumentoDto
            {
                IdDocumento = d.IdDocumento,
                ProyectoId = d.ProyectoId,
                NombreDocumento = d.NombreDocumento,
                CategoriaDocumento = d.CategoriaDocumento,
                Descripcion = d.Descripcion,
                TipoDocumento = d.TipoDocumento,
                VersionActual = d.VersionActual,
                TotalVersiones = d.TotalVersiones,
                NombreArchivoOriginal = d.NombreArchivoOriginal,
                RutaBlobCompleta = d.RutaBlobCompleta,
                TipoArchivo = d.TipoArchivo,
                TamanoBytes = d.TamanoBytes,
                SubidoPor = d.SubidoPor,
                NombreCreadoPor = nombresUsuarios.GetValueOrDefault(d.SubidoPor, "Usuario desconocido"),
                FechaSubida = d.FechaSubida,
                UltimaModificacion = d.UltimaModificacion,
                Versiones = versiones
                    .Where(v => v.DocumentoId == d.IdDocumento)
                    .Select(v => new DocumentoVersionDto
                    {
                        IdVersion = v.IdVersion,
                        DocumentoId = v.DocumentoId,
                        NumeroVersion = v.NumeroVersion,
                        EsVersionActual = v.EsVersionActual,
                        NombreArchivoOriginal = v.NombreArchivoOriginal,
                        NombreBlob = v.NombreBlob,
                        RutaBlobCompleta = v.RutaBlobCompleta,
                        TipoArchivo = v.TipoArchivo,
                        TamanoBytes = v.TamanoBytes,
                        SubidoPor = v.SubidoPor,
                        NombreSubidoPor = nombresUsuarios.GetValueOrDefault(v.SubidoPor, "Usuario desconocido"),
                        FechaSubida = v.FechaSubida,
                        Comentarios = v.Comentarios,
                        NombreDocumento = d.NombreDocumento,
                        CategoriaDocumento = d.CategoriaDocumento
                    }).ToList()
            }).ToList();
        }

        public async Task<DocumentoDto?> ObtenerDocumentoPorIdAsync(int idDocumento)
        {
            var documento = await _context.Documentos
            .FirstOrDefaultAsync(d => d.IdDocumento == idDocumento);

            if (documento == null) return null;

            var versiones = await _context.DocumentoVersiones
                .Where(v => v.DocumentoId == idDocumento && v.Activo)
                .OrderByDescending(v => v.NumeroVersion)
                .ToListAsync();

            var todosUsuarioIds = new[] { documento.SubidoPor }
                .Concat(versiones.Select(v => v.SubidoPor))
                .ToList();

            var nombresUsuarios = await ObtenerNombresUsuariosAsync(todosUsuarioIds);

            return new DocumentoDto
            {
                IdDocumento = documento.IdDocumento,
                ProyectoId = documento.ProyectoId,
                NombreDocumento = documento.NombreDocumento,
                CategoriaDocumento = documento.CategoriaDocumento,
                Descripcion = documento.Descripcion,
                TipoDocumento = documento.TipoDocumento,
                VersionActual = documento.VersionActual,
                TotalVersiones = documento.TotalVersiones,
                NombreArchivoOriginal = documento.NombreArchivoOriginal,
                RutaBlobCompleta = documento.RutaBlobCompleta,
                TipoArchivo = documento.TipoArchivo,
                TamanoBytes = documento.TamanoBytes,
                SubidoPor = documento.SubidoPor,
                NombreCreadoPor = nombresUsuarios.GetValueOrDefault(documento.SubidoPor, "Usuario desconocido"),
                FechaSubida = documento.FechaSubida,
                UltimaModificacion = documento.UltimaModificacion,
                Versiones = versiones.Select(v => new DocumentoVersionDto
                {
                    IdVersion = v.IdVersion,
                    DocumentoId = v.DocumentoId,
                    NumeroVersion = v.NumeroVersion,
                    EsVersionActual = v.EsVersionActual,
                    NombreArchivoOriginal = v.NombreArchivoOriginal,
                    NombreBlob = v.NombreBlob,
                    RutaBlobCompleta = v.RutaBlobCompleta,
                    TipoArchivo = v.TipoArchivo,
                    TamanoBytes = v.TamanoBytes,
                    SubidoPor = v.SubidoPor,
                    NombreSubidoPor = nombresUsuarios.GetValueOrDefault(v.SubidoPor, "Usuario desconocido"),
                    FechaSubida = v.FechaSubida,
                    Comentarios = v.Comentarios,
                    NombreDocumento = documento.NombreDocumento,
                    CategoriaDocumento = documento.CategoriaDocumento
                }).ToList()
            };
        }

        public async Task<DocumentoVersionDto?> ObtenerVersionPorIdAsync(int idVersion)
        {
            var version = await _context.DocumentoVersiones
            .FirstOrDefaultAsync(v => v.IdVersion == idVersion);

            if (version == null) return null;

            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.IdDocumento == version.DocumentoId);

            if (documento == null) return null;

            var nombresUsuarios = await ObtenerNombresUsuariosAsync(new[] { version.SubidoPor });

            return new DocumentoVersionDto
            {
                IdVersion = version.IdVersion,
                DocumentoId = version.DocumentoId,
                NumeroVersion = version.NumeroVersion,
                EsVersionActual = version.EsVersionActual,
                NombreArchivoOriginal = version.NombreArchivoOriginal,
                NombreBlob = version.NombreBlob,
                RutaBlobCompleta = version.RutaBlobCompleta,
                TipoArchivo = version.TipoArchivo,
                TamanoBytes = version.TamanoBytes,
                SubidoPor = version.SubidoPor,
                NombreSubidoPor = nombresUsuarios.GetValueOrDefault(version.SubidoPor, "Usuario desconocido"),
                FechaSubida = version.FechaSubida,
                Comentarios = version.Comentarios,
                NombreDocumento = documento.NombreDocumento,
                CategoriaDocumento = documento.CategoriaDocumento
            };
        }

        public async Task<IEnumerable<DocumentoVersionDto>> ObtenerVersionesPorDocumentoAsync(int documentoId)
        {
            // 1. Obtener el documento padre
            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.IdDocumento == documentoId);

            if (documento == null)
                return new List<DocumentoVersionDto>();

            // 2. Obtener todas las versiones del documento
            var versiones = await _context.DocumentoVersiones
                .Where(v => v.DocumentoId == documentoId && v.Activo)
                .OrderByDescending(v => v.NumeroVersion)
                .ToListAsync();

            if (!versiones.Any())
                return new List<DocumentoVersionDto>();

            // 3. Obtener nombres de usuarios
            var usuarioIds = versiones.Select(v => v.SubidoPor).ToList();
            var nombresUsuarios = await ObtenerNombresUsuariosAsync(usuarioIds);

            // 4. Mapear a DTOs
            return versiones.Select(v => new DocumentoVersionDto
            {
                IdVersion = v.IdVersion,
                DocumentoId = v.DocumentoId,
                NumeroVersion = v.NumeroVersion,
                EsVersionActual = v.EsVersionActual,
                NombreArchivoOriginal = v.NombreArchivoOriginal,
                NombreBlob = v.NombreBlob,
                RutaBlobCompleta = v.RutaBlobCompleta,
                TipoArchivo = v.TipoArchivo,
                TamanoBytes = v.TamanoBytes,
                SubidoPor = v.SubidoPor,
                NombreSubidoPor = nombresUsuarios.GetValueOrDefault(v.SubidoPor, "Usuario desconocido"),
                FechaSubida = v.FechaSubida,
                Comentarios = v.Comentarios,
                NombreDocumento = documento.NombreDocumento,
                CategoriaDocumento = documento.CategoriaDocumento
            }).ToList();
        }

        public async Task<DocumentoVersionDto?> ObtenerVersionActualAsync(int documentoId)
        {
            var version = await _context.DocumentoVersiones
            .FirstOrDefaultAsync(v => v.DocumentoId == documentoId && v.EsVersionActual);

            if (version == null) return null;

            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.IdDocumento == documentoId);

            if (documento == null) return null;

            var nombresUsuarios = await ObtenerNombresUsuariosAsync(new[] { version.SubidoPor });

            return new DocumentoVersionDto
            {
                IdVersion = version.IdVersion,
                DocumentoId = version.DocumentoId,
                NumeroVersion = version.NumeroVersion,
                EsVersionActual = version.EsVersionActual,
                NombreArchivoOriginal = version.NombreArchivoOriginal,
                NombreBlob = version.NombreBlob,
                RutaBlobCompleta = version.RutaBlobCompleta,
                TipoArchivo = version.TipoArchivo,
                TamanoBytes = version.TamanoBytes,
                SubidoPor = version.SubidoPor,
                NombreSubidoPor = nombresUsuarios.GetValueOrDefault(version.SubidoPor, "Usuario desconocido"),
                FechaSubida = version.FechaSubida,
                Comentarios = version.Comentarios,
                NombreDocumento = documento.NombreDocumento,
                CategoriaDocumento = documento.CategoriaDocumento
            };
        }

        // ==========================================
        // OPERACIONES
        // ==========================================
        public async Task MarcarVersionComoActualAsync(int idVersion)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var version = await _context.DocumentoVersiones
                    .FirstOrDefaultAsync(v => v.IdVersion == idVersion);

                if (version == null)
                    throw new InvalidOperationException("Versión no encontrada.");

                var documento = await _context.Documentos
                    .FirstOrDefaultAsync(d => d.IdDocumento == version.DocumentoId);

                if (documento == null)
                    throw new InvalidOperationException("Documento no encontrado.");

                var todasVersiones = await _context.DocumentoVersiones
                    .Where(v => v.DocumentoId == version.DocumentoId)
                    .ToListAsync();

                foreach (var v in todasVersiones)
                {
                    v.EsVersionActual = false;
                }

                version.EsVersionActual = true;
                documento.VersionActual = version.NumeroVersion;
                documento.UltimaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EliminarDocumentoAsync(int idDocumento)
        {
            var documento = await _context.Documentos
            .FirstOrDefaultAsync(d => d.IdDocumento == idDocumento);

            if (documento == null)
                throw new InvalidOperationException("Documento no encontrado.");

            documento.Activo = false;

            var versiones = await _context.DocumentoVersiones
                .Where(v => v.DocumentoId == idDocumento)
                .ToListAsync();

            foreach (var version in versiones)
            {
                version.Activo = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task EliminarVersionAsync(int idVersion)
        {
            var version = await _context.DocumentoVersiones
            .FirstOrDefaultAsync(v => v.IdVersion == idVersion);

            if (version == null)
                throw new InvalidOperationException("Versión no encontrada.");

            if (version.EsVersionActual)
                throw new InvalidOperationException("No se puede eliminar la versión actual.");

            version.Activo = false;
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> ObtenerSiguienteNumeroVersionAsync(int documentoId)
        {
            var ultimaVersion = await _context.DocumentoVersiones
            .Where(v => v.DocumentoId == documentoId && v.Activo)
            .OrderByDescending(v => v.NumeroVersion)
            .Select(v => v.NumeroVersion)
            .FirstOrDefaultAsync();

            if (ultimaVersion == 0) return 1.0m;

            return ultimaVersion + 0.1m;
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private string ExtraerNombreBlob(string rutaBlobCompleta)
        {
            var uri = new Uri(rutaBlobCompleta);
            return uri.Segments.Last();
        }
    }
}
