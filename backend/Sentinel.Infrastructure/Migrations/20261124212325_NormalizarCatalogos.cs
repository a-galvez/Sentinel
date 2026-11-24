using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sentinel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AplicaA",
                table: "TiposDocumento",
                newName: "AmbitoDocumentoId");

            migrationBuilder.RenameColumn(
                name: "TipoRequisito",
                table: "Requisitos",
                newName: "TipoRequisitoId");

            migrationBuilder.CreateTable(
                name: "AmbitosDocumento",
                columns: table => new
                {
                    AmbitoDocumentoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmbitosDocumento", x => x.AmbitoDocumentoId);
                });

            migrationBuilder.CreateTable(
                name: "TiposRequisito",
                columns: table => new
                {
                    TipoRequisitoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposRequisito", x => x.TipoRequisitoId);
                });

            // Las columnas renombradas conservan los valores de los enums viejos
            // (AplicaA: 0/1, TipoRequisito: 0/1/2). Los IDs del catálogo nuevo empiezan en 1,
            // así que se siembran las filas y se desplazan los valores antes de crear las FKs.
            migrationBuilder.Sql("""
                INSERT INTO "AmbitosDocumento" ("AmbitoDocumentoId", "Nombre")
                VALUES (1, 'Producto'), (2, 'Establecimiento')
                ON CONFLICT ("AmbitoDocumentoId") DO NOTHING;

                INSERT INTO "TiposRequisito" ("TipoRequisitoId", "Nombre")
                VALUES (1, 'Documento'), (2, 'Dato'), (3, 'Acción')
                ON CONFLICT ("TipoRequisitoId") DO NOTHING;

                UPDATE "TiposDocumento"
                SET "AmbitoDocumentoId" = "AmbitoDocumentoId" + 1;

                UPDATE "Requisitos"
                SET "TipoRequisitoId" = "TipoRequisitoId" + 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumento_AmbitoDocumentoId",
                table: "TiposDocumento",
                column: "AmbitoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitos_TipoRequisitoId",
                table: "Requisitos",
                column: "TipoRequisitoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitos_TiposRequisito_TipoRequisitoId",
                table: "Requisitos",
                column: "TipoRequisitoId",
                principalTable: "TiposRequisito",
                principalColumn: "TipoRequisitoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TiposDocumento_AmbitosDocumento_AmbitoDocumentoId",
                table: "TiposDocumento",
                column: "AmbitoDocumentoId",
                principalTable: "AmbitosDocumento",
                principalColumn: "AmbitoDocumentoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitos_TiposRequisito_TipoRequisitoId",
                table: "Requisitos");

            migrationBuilder.DropForeignKey(
                name: "FK_TiposDocumento_AmbitosDocumento_AmbitoDocumentoId",
                table: "TiposDocumento");

            // Revierte el desplazamiento aplicado en Up para volver a los valores de enum.
            migrationBuilder.Sql("""
                UPDATE "TiposDocumento"
                SET "AmbitoDocumentoId" = "AmbitoDocumentoId" - 1;

                UPDATE "Requisitos"
                SET "TipoRequisitoId" = "TipoRequisitoId" - 1;
                """);

            migrationBuilder.DropTable(
                name: "AmbitosDocumento");

            migrationBuilder.DropTable(
                name: "TiposRequisito");

            migrationBuilder.DropIndex(
                name: "IX_TiposDocumento_AmbitoDocumentoId",
                table: "TiposDocumento");

            migrationBuilder.DropIndex(
                name: "IX_Requisitos_TipoRequisitoId",
                table: "Requisitos");

            migrationBuilder.RenameColumn(
                name: "AmbitoDocumentoId",
                table: "TiposDocumento",
                newName: "AplicaA");

            migrationBuilder.RenameColumn(
                name: "TipoRequisitoId",
                table: "Requisitos",
                newName: "TipoRequisito");
        }
    }
}
