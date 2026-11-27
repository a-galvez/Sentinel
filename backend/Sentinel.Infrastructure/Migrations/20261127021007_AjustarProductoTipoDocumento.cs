using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustarProductoTipoDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_TiposDocumento_TipoDocumentoId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_TipoDocumentoId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoId",
                table: "Productos");

            migrationBuilder.AlterColumn<int>(
                name: "TipoDocumentoId",
                table: "RegistrosSanitarios",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "ChecklistRequisitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Orden",
                table: "ChecklistRequisitos");

            migrationBuilder.AlterColumn<int>(
                name: "TipoDocumentoId",
                table: "RegistrosSanitarios",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDocumentoId",
                table: "Productos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TipoDocumentoId",
                table: "Productos",
                column: "TipoDocumentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_TiposDocumento_TipoDocumentoId",
                table: "Productos",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "TipoDocumentoId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
