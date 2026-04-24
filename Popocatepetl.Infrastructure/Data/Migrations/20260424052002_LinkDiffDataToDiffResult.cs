using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Popocatepetl.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkDiffDataToDiffResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiffResultId",
                table: "DiffData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiffData_DiffResultId",
                table: "DiffData",
                column: "DiffResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiffData_DiffResults_DiffResultId",
                table: "DiffData",
                column: "DiffResultId",
                principalTable: "DiffResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiffData_DiffResults_DiffResultId",
                table: "DiffData");

            migrationBuilder.DropIndex(
                name: "IX_DiffData_DiffResultId",
                table: "DiffData");

            migrationBuilder.DropColumn(
                name: "DiffResultId",
                table: "DiffData");
        }
    }
}
