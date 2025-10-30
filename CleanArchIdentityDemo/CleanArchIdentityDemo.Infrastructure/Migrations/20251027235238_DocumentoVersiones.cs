using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchIdentityDemo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentoVersiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Documentos",
                newName: "TotalVersiones");

            migrationBuilder.RenameColumn(
                name: "RutaArchivo",
                table: "Documentos",
                newName: "TipoDocumento");

            migrationBuilder.RenameColumn(
                name: "EsVersionGeneral",
                table: "Documentos",
                newName: "Activo");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreArchivoOriginal",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreBlob",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaBlobCompleta",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TamanoBytes",
                table: "Documentos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoArchivo",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaModificacion",
                table: "Documentos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "VersionActual",
                table: "Documentos",
                type: "decimal(3,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DocumentoVersiones",
                columns: table => new
                {
                    IdVersion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    NumeroVersion = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    EsVersionActual = table.Column<bool>(type: "bit", nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreBlob = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaBlobCompleta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    SubidoPor = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoVersiones", x => x.IdVersion);
                    table.ForeignKey(
                        name: "FK_DocumentoVersiones_AspNetUsers_SubidoPor",
                        column: x => x.SubidoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoVersiones_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "IdDocumento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoVersiones_DocumentoId",
                table: "DocumentoVersiones",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoVersiones_SubidoPor",
                table: "DocumentoVersiones",
                column: "SubidoPor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentoVersiones");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "NombreArchivoOriginal",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "NombreBlob",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "RutaBlobCompleta",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "TamanoBytes",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "TipoArchivo",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "UltimaModificacion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "VersionActual",
                table: "Documentos");

            migrationBuilder.RenameColumn(
                name: "TotalVersiones",
                table: "Documentos",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "TipoDocumento",
                table: "Documentos",
                newName: "RutaArchivo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Documentos",
                newName: "EsVersionGeneral");
        }
    }
}
