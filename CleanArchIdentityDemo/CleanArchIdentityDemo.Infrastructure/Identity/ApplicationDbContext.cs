using CleanArchIdentityDemo.Application.Interfaces;
using CleanArchIdentityDemo.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 🔹 Obtener usuario actual
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Si no hay usuario autenticado, usar "Sistema"
            if (string.IsNullOrEmpty(userId))
            {
                var sistemaUser = await Set<ApplicationUser>().FirstOrDefaultAsync(u => u.UserName == "Sistema");
                if (sistemaUser != null)
                    userId = sistemaUser.Id;
                else
                {
                    //  No auditar si el usuario Sistema aún no existe
                    return await base.SaveChangesAsync(cancellationToken);
                }
            }

            // 🔹 Detectar cambios
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditoriaAccion &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList(); // ✅ fuerza evaluación antes del foreach

            // 🔹 Crear una lista temporal para evitar modificar la colección durante la iteración
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

                auditorias.Add(new AuditoriaAccion
                {
                    UsuarioId = userId ?? "Sistema",
                    Modulo = modulo,
                    Accion = accion,
                    FechaHora = DateTime.Now
                });
            }

            // ✅ Ahora agregamos todas las auditorías al final (fuera del foreach)
            if (auditorias.Any())
                await AuditoriaAcciones.AddRangeAsync(auditorias, cancellationToken);

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
