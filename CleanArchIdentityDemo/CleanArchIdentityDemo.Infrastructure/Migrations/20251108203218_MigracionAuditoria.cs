using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchIdentityDemo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigracionAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditoriaAcciones_AspNetUsers_UsuarioId",
                table: "AuditoriaAcciones");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "AuditoriaAcciones",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DatoAnterior",
                table: "AuditoriaAcciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatoNuevo",
                table: "AuditoriaAcciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditoriaAcciones_AspNetUsers_UsuarioId",
                table: "AuditoriaAcciones",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditoriaAcciones_AspNetUsers_UsuarioId",
                table: "AuditoriaAcciones");

            migrationBuilder.DropColumn(
                name: "DatoAnterior",
                table: "AuditoriaAcciones");

            migrationBuilder.DropColumn(
                name: "DatoNuevo",
                table: "AuditoriaAcciones");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "AuditoriaAcciones",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditoriaAcciones_AspNetUsers_UsuarioId",
                table: "AuditoriaAcciones",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
