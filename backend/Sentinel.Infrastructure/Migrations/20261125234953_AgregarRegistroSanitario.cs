using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroSanitario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TiposDocumento_Codigo",
                table: "TiposDocumento");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId_NumeroRegistro",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaEmision",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "NumeroRegistro",
                table: "Productos");

            migrationBuilder.CreateTable(
                name: "RegistrosSanitarios",
                columns: table => new
                {
                    RegistroSanitarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoDocumentoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroRegistro = table.Column<string>(type: "text", nullable: true),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    DadoDeBaja = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosSanitarios", x => x.RegistroSanitarioId);
                    table.ForeignKey(
                        name: "FK_RegistrosSanitarios_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "ProductoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosSanitarios_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "TipoDocumentoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSanitarios_EmpresaId_NumeroRegistro",
                table: "RegistrosSanitarios",
                columns: new[] { "EmpresaId", "NumeroRegistro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSanitarios_ProductoId",
                table: "RegistrosSanitarios",
                column: "ProductoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSanitarios_TipoDocumentoId",
                table: "RegistrosSanitarios",
                column: "TipoDocumentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosSanitarios");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaEmision",
                table: "Productos",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaVencimiento",
                table: "Productos",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "NumeroRegistro",
                table: "Productos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumento_Codigo",
                table: "TiposDocumento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId_NumeroRegistro",
                table: "Productos",
                columns: new[] { "EmpresaId", "NumeroRegistro" },
                unique: true);
        }
    }
}
