using CleanArchIdentityDemo.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CleanArchIdentityDemo.Infrastructure.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Servicio para obtener el usuario actual
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
          : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        // DbSets por cada entidad del dominio
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<EstadoProyecto> EstadosProyecto { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<DocumentoVersion> DocumentoVersiones { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<PersonalProyecto> PersonalProyecto { get; set; }
        public DbSet<HoraLaboral> HorasLaborales { get; set; }
        public DbSet<NotaAvance> NotasAvance { get; set; }
        public DbSet<Incidente> Incidentes { get; set; }
        public DbSet<SolicitudMaterial> SolicitudesMaterial { get; set; }
        public DbSet<MaterialSolicitado> MaterialesSolicitados { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<MaterialProyecto> MaterialesProyecto { get; set; }
        public DbSet<DevolucionMaterial> DevolucionesMaterial { get; set; }
        public DbSet<CostoEjecutado> CostosEjecutados { get; set; }
        public DbSet<PagoProveedor> PagosProveedores { get; set; }
        public DbSet<MantenimientoMaquinaria> MantenimientosMaquinaria { get; set; }
        public DbSet<Maquinaria> Maquinarias { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<AuditoriaAccion> AuditoriaAcciones { get; set; }
        public DbSet<AccesoModulo> AccesosModulo { get; set; }
        public DbSet<MaquinariaProyecto> MaquinariaProyecto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración genérica: TODAS las FKs en Cascade
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Cascade;
            }

            //Tablas con nombres explícitos
            modelBuilder.Entity<Proyecto>().ToTable("Proyectos");
            modelBuilder.Entity<EstadoProyecto>().ToTable("EstadosProyecto");
            modelBuilder.Entity<Documento>().ToTable("Documentos");
            modelBuilder.Entity<DocumentoVersion>().ToTable("DocumentoVersiones");
            modelBuilder.Entity<Tarea>().ToTable("Tareas");
            modelBuilder.Entity<PersonalProyecto>().ToTable("PersonalProyecto");
            modelBuilder.Entity<HoraLaboral>().ToTable("HorasLaborales");
            modelBuilder.Entity<NotaAvance>().ToTable("NotasAvance");
            modelBuilder.Entity<Incidente>().ToTable("Incidentes");
            modelBuilder.Entity<SolicitudMaterial>().ToTable("SolicitudesMaterial");
            modelBuilder.Entity<MaterialSolicitado>().ToTable("MaterialesSolicitados");
            modelBuilder.Entity<Material>().ToTable("Materiales");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedores");
            modelBuilder.Entity<MaterialProyecto>().ToTable("MaterialesProyecto");
            modelBuilder.Entity<DevolucionMaterial>().ToTable("DevolucionesMaterial");
            modelBuilder.Entity<CostoEjecutado>().ToTable("CostosEjecutados");
            modelBuilder.Entity<PagoProveedor>().ToTable("PagosProveedores");
            modelBuilder.Entity<MantenimientoMaquinaria>().ToTable("MantenimientosMaquinaria");
            modelBuilder.Entity<Maquinaria>().ToTable("Maquinarias");
            modelBuilder.Entity<Contrato>().ToTable("Contratos");
            modelBuilder.Entity<AuditoriaAccion>().ToTable("AuditoriaAcciones");
            modelBuilder.Entity<AccesoModulo>().ToTable("AccesosModulo");
            modelBuilder.Entity<MaquinariaProyecto>().ToTable("MaquinariaProyecto");

            //Relaciones específicas

            // Documento -> ApplicationUser (SubidoPor)
            modelBuilder.Entity<Documento>()
                .HasOne<ApplicationUser>() // no se usa propiedad de navegación en Domain
                .WithMany()                // no se declara colección en ApplicationUser
                .HasForeignKey(d => d.SubidoPor); // FK string en Documento

            // DocumentoVersion -> ApplicationUser (SubidoPor)
            modelBuilder.Entity<DocumentoVersion>()
                .HasOne<ApplicationUser>() // no se usa propiedad de navegación en Domain
                .WithMany()                // no se declara colección en ApplicationUser
                .HasForeignKey(d => d.SubidoPor); // FK string en DocumentoVersion

            // DocumentoVersion -> Documento evitar cascada múltiple y cycles con OnDelete.Restrict
            modelBuilder.Entity<DocumentoVersion>()
            .HasOne(dv => dv.Documento)
            .WithMany(d => d.Versiones)
            .HasForeignKey(dv => dv.DocumentoId)
            .OnDelete(DeleteBehavior.Restrict); // evitar cascada múltiple y cycles

            // Incidente -> ApplicationUser (CreadoPor)
            modelBuilder.Entity<Incidente>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(i => i.CreadoPor);


            // NotaAvance -> ApplicationUser (CreadoPor)
            modelBuilder.Entity<NotaAvance>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(n => n.CreadoPor);


            // AccesoModulo -> ApplicationUser
            modelBuilder.Entity<AccesoModulo>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UsuarioId);


            // AuditoriaAccion -> ApplicationUser
            modelBuilder.Entity<AuditoriaAccion>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // PersonalProyecto -> ApplicationUser
            modelBuilder.Entity<PersonalProyecto>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.UsuarioId);
        }

        private string FormatearDiccionario(Dictionary<string, string> datos)
        {
            return string.Join("\n", datos.Select(d => $"{d.Key}: {d.Value}"));
        }

        private string FormatearCambios(Dictionary<string, (string Antes, string Despues)> cambios)
        {
            return string.Join("\n", cambios.Select(c =>
                $"{c.Key}: Antes = {c.Value.Antes} | Después = {c.Value.Despues}"
            ));
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // verificar si hay un usuario autenticado
            var usuarioId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var tieneUsuarioAutenticado = !string.IsNullOrEmpty(usuarioId);

            // auditar solo si hay usuario autenticado

            if (tieneUsuarioAutenticado)
            {
                // Detectar cambios
                var entries = ChangeTracker.Entries()
                    .Where(e => e.Entity is not AuditoriaAccion &&
                                (e.State == EntityState.Added ||
                                 e.State == EntityState.Modified ||
                                 e.State == EntityState.Deleted))
                    .ToList(); // fuerza evaluación antes del foreach

                // Crear una lista temporal para evitar modificar la colección durante la iteración
                var auditorias = new List<AuditoriaAccion>();

                foreach (var entry in entries)
                {
                    string accion = entry.State switch
                    {
                        EntityState.Added => "Creación",
                        EntityState.Modified => "Modificación",
                        EntityState.Deleted => "Eliminación",
                        _ => "Desconocida"
                    };

                    string modulo = entry.Entity.GetType().Name;

                    string datosAnteriores = "";
                    string datosNuevos = "";

                    if (entry.State == EntityState.Modified)
                    {
                        var cambios = new Dictionary<string, object>();

                        foreach (var prop in entry.OriginalValues.Properties)
                        {
                            var original = entry.OriginalValues[prop]?.ToString();
                            var current = entry.CurrentValues[prop]?.ToString();

                            if (original != current) // solo propiedades que cambiaron
                            {
                                cambios[prop.Name] = new
                                {
                                    Antes = original,
                                    Despues = current
                                };
                            }
                        }

                        if (cambios.Any())
                        {
                            datosAnteriores = JsonSerializer.Serialize(
                                cambios.ToDictionary(c => c.Key, c => ((dynamic)c.Value).Antes)
                            );
                            datosNuevos = JsonSerializer.Serialize(
                                cambios.ToDictionary(c => c.Key, c => ((dynamic)c.Value).Despues)
                            );
                        }
                    }
                    else if (entry.State == EntityState.Added)
                    {
                        var nuevos = entry.CurrentValues.Properties.ToDictionary(
                            p => p.Name,
                            p => entry.CurrentValues[p]?.ToString()
                        );
                        datosNuevos = JsonSerializer.Serialize(nuevos);
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        var antiguos = entry.OriginalValues.Properties.ToDictionary(
                            p => p.Name,
                            p => entry.OriginalValues[p]?.ToString()
                        );
                        datosAnteriores = JsonSerializer.Serialize(antiguos);
                    }

                    auditorias.Add(new AuditoriaAccion
                    {
                        UsuarioId = usuarioId ?? "Sistema",
                        Modulo = modulo,
                        Accion = accion,
                        FechaHora = DateTime.Now,
                        DatoAnterior = datosAnteriores,
                        DatoNuevo = datosNuevos
                    });
                }

                if (auditorias.Any())
                    await AuditoriaAcciones.AddRangeAsync(auditorias, cancellationToken);

            }
            // Se agregan todas las auditorías al final (fuera del foreach)
            return await base.SaveChangesAsync(cancellationToken);
        }


    }

}
