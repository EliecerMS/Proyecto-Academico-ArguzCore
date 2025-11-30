using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchIdentityDemo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProyectoEntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioPropuesta",
                table: "Proyectos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaInicioPropuesta",
                table: "Proyectos");
        }
    }
}
