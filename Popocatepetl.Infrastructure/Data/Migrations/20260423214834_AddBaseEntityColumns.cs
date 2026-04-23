using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Popocatepetl.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DiffResults",
                table: "DiffResults");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "AppSettings",
                newName: "Type");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Reports",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DiffResults",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DiffResults",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DiffData",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AuditLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiffResults",
                table: "DiffResults",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DiffResults_BaselineReportId",
                table: "DiffResults",
                column: "BaselineReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DiffResults",
                table: "DiffResults");

            migrationBuilder.DropIndex(
                name: "IX_DiffResults_BaselineReportId",
                table: "DiffResults");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DiffResults");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DiffResults");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DiffData");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "AppSettings",
                newName: "Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiffResults",
                table: "DiffResults",
                columns: new[] { "BaselineReportId", "CurrentReportId" });
        }
    }
}
