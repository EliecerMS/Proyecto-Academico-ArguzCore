using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchIdentityDemo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablasProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccesosModulo",
                columns: table => new
                {
                    IdAccesoModulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccesosModulo", x => x.IdAccesoModulo);
                    table.ForeignKey(
                        name: "FK_AccesosModulo_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaAcciones",
                columns: table => new
                {
                    IdAuditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaAcciones", x => x.IdAuditoria);
                    table.ForeignKey(
                        name: "FK_AuditoriaAcciones_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstadosProyecto",
                columns: table => new
                {
                    IdEstadoProyecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosProyecto", x => x.IdEstadoProyecto);
                });

            migrationBuilder.CreateTable(
                name: "Maquinarias",
                columns: table => new
                {
                    IdMaquinaria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoMaquinaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroSerie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maquinarias", x => x.IdMaquinaria);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    IdProveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreProveedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.IdProveedor);
                });

            migrationBuilder.CreateTable(
                name: "Proyectos",
                columns: table => new
                {
                    IdProyecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoProyecto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaFinalPropuesta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Presupuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstadoProyectoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.IdProyecto);
                    table.ForeignKey(
                        name: "FK_Proyectos_EstadosProyecto_EstadoProyectoId",
                        column: x => x.EstadoProyectoId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "IdEstadoProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MantenimientosMaquinaria",
                columns: table => new
                {
                    IdMantenimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaquinariaId = table.Column<int>(type: "int", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MantenimientosMaquinaria", x => x.IdMantenimiento);
                    table.ForeignKey(
                        name: "FK_MantenimientosMaquinaria_Maquinarias_MaquinariaId",
                        column: x => x.MaquinariaId,
                        principalTable: "Maquinarias",
                        principalColumn: "IdMaquinaria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Materiales",
                columns: table => new
                {
                    IdMaterial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreMaterial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadDisponible = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiales", x => x.IdMaterial);
                    table.ForeignKey(
                        name: "FK_Materiales_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "IdProveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    IdContrato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RutaDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.IdContrato);
                    table.ForeignKey(
                        name: "FK_Contratos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CostosEjecutados",
                columns: table => new
                {
                    IdCosto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    CategoriaGasto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaComprobante = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostosEjecutados", x => x.IdCosto);
                    table.ForeignKey(
                        name: "FK_CostosEjecutados_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    IdDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    NombreDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoriaDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubidoPor = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EsVersionGeneral = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.IdDocumento);
                    table.ForeignKey(
                        name: "FK_Documentos_AspNetUsers_SubidoPor",
                        column: x => x.SubidoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documentos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidentes",
                columns: table => new
                {
                    IdIncidente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidentes", x => x.IdIncidente);
                    table.ForeignKey(
                        name: "FK_Incidentes_AspNetUsers_CreadoPor",
                        column: x => x.CreadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidentes_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaquinariaProyecto",
                columns: table => new
                {
                    IdMaquinariaProyecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaquinariaId = table.Column<int>(type: "int", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaquinariaProyecto", x => x.IdMaquinariaProyecto);
                    table.ForeignKey(
                        name: "FK_MaquinariaProyecto_Maquinarias_MaquinariaId",
                        column: x => x.MaquinariaId,
                        principalTable: "Maquinarias",
                        principalColumn: "IdMaquinaria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaquinariaProyecto_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotasAvance",
                columns: table => new
                {
                    IdNota = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destacada = table.Column<bool>(type: "bit", nullable: false),
                    FechaNota = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasAvance", x => x.IdNota);
                    table.ForeignKey(
                        name: "FK_NotasAvance_AspNetUsers_CreadoPor",
                        column: x => x.CreadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasAvance_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosProveedores",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaComprobante = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosProveedores", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_PagosProveedores_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "IdProveedor",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagosProveedores_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalProyecto",
                columns: table => new
                {
                    IdPersonalProyecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalProyecto", x => x.IdPersonalProyecto);
                    table.ForeignKey(
                        name: "FK_PersonalProyecto_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalProyecto_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesMaterial",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoSolicitud = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObservacionesBodeguero = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesMaterial", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_SolicitudesMaterial_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    IdTarea = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    NombreTarea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicioEsperada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalEsperada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PorcentajeAvance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.IdTarea);
                    table.ForeignKey(
                        name: "FK_Tareas_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialesProyecto",
                columns: table => new
                {
                    IdMaterialProyecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    CantidadEnObra = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialesProyecto", x => x.IdMaterialProyecto);
                    table.ForeignKey(
                        name: "FK_MaterialesProyecto_Materiales_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiales",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialesProyecto_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorasLaborales",
                columns: table => new
                {
                    IdHoraLaboral = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalProyectoId = table.Column<int>(type: "int", nullable: false),
                    HoraEntrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorasLaborales", x => x.IdHoraLaboral);
                    table.ForeignKey(
                        name: "FK_HorasLaborales_PersonalProyecto_PersonalProyectoId",
                        column: x => x.PersonalProyectoId,
                        principalTable: "PersonalProyecto",
                        principalColumn: "IdPersonalProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialesSolicitados",
                columns: table => new
                {
                    IdMaterialSolicitado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialesSolicitados", x => x.IdMaterialSolicitado);
                    table.ForeignKey(
                        name: "FK_MaterialesSolicitados_Materiales_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiales",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialesSolicitados_SolicitudesMaterial_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "SolicitudesMaterial",
                        principalColumn: "IdSolicitud",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevolucionesMaterial",
                columns: table => new
                {
                    IdDevolucion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialProyectoId = table.Column<int>(type: "int", nullable: false),
                    CantidadDevuelta = table.Column<int>(type: "int", nullable: false),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesMaterial", x => x.IdDevolucion);
                    table.ForeignKey(
                        name: "FK_DevolucionesMaterial_MaterialesProyecto_MaterialProyectoId",
                        column: x => x.MaterialProyectoId,
                        principalTable: "MaterialesProyecto",
                        principalColumn: "IdMaterialProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccesosModulo_UsuarioId",
                table: "AccesosModulo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_UsuarioId",
                table: "AuditoriaAcciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_ProyectoId",
                table: "Contratos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosEjecutados_ProyectoId",
                table: "CostosEjecutados",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMaterial_MaterialProyectoId",
                table: "DevolucionesMaterial",
                column: "MaterialProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ProyectoId",
                table: "Documentos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_SubidoPor",
                table: "Documentos",
                column: "SubidoPor");

            migrationBuilder.CreateIndex(
                name: "IX_HorasLaborales_PersonalProyectoId",
                table: "HorasLaborales",
                column: "PersonalProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_CreadoPor",
                table: "Incidentes",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_ProyectoId",
                table: "Incidentes",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientosMaquinaria_MaquinariaId",
                table: "MantenimientosMaquinaria",
                column: "MaquinariaId");

            migrationBuilder.CreateIndex(
                name: "IX_MaquinariaProyecto_MaquinariaId",
                table: "MaquinariaProyecto",
                column: "MaquinariaId");

            migrationBuilder.CreateIndex(
                name: "IX_MaquinariaProyecto_ProyectoId",
                table: "MaquinariaProyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Materiales_ProveedorId",
                table: "Materiales",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialesProyecto_MaterialId",
                table: "MaterialesProyecto",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialesProyecto_ProyectoId",
                table: "MaterialesProyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialesSolicitados_MaterialId",
                table: "MaterialesSolicitados",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialesSolicitados_SolicitudId",
                table: "MaterialesSolicitados",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasAvance_CreadoPor",
                table: "NotasAvance",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_NotasAvance_ProyectoId",
                table: "NotasAvance",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_ProveedorId",
                table: "PagosProveedores",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_ProyectoId",
                table: "PagosProveedores",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalProyecto_ProyectoId",
                table: "PersonalProyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalProyecto_UsuarioId",
                table: "PersonalProyecto",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_EstadoProyectoId",
                table: "Proyectos",
                column: "EstadoProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMaterial_ProyectoId",
                table: "SolicitudesMaterial",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_ProyectoId",
                table: "Tareas",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccesosModulo");

            migrationBuilder.DropTable(
                name: "AuditoriaAcciones");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "CostosEjecutados");

            migrationBuilder.DropTable(
                name: "DevolucionesMaterial");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "HorasLaborales");

            migrationBuilder.DropTable(
                name: "Incidentes");

            migrationBuilder.DropTable(
                name: "MantenimientosMaquinaria");

            migrationBuilder.DropTable(
                name: "MaquinariaProyecto");

            migrationBuilder.DropTable(
                name: "MaterialesSolicitados");

            migrationBuilder.DropTable(
                name: "NotasAvance");

            migrationBuilder.DropTable(
                name: "PagosProveedores");

            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "MaterialesProyecto");

            migrationBuilder.DropTable(
                name: "PersonalProyecto");

            migrationBuilder.DropTable(
                name: "Maquinarias");

            migrationBuilder.DropTable(
                name: "SolicitudesMaterial");

            migrationBuilder.DropTable(
                name: "Materiales");

            migrationBuilder.DropTable(
                name: "Proyectos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "EstadosProyecto");
        }
    }
}
