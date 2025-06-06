using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assistants_TransportUnits_TransportUnitId",
                table: "Assistants");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Loads",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Loads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Loads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assistants_TransportUnits_TransportUnitId",
                table: "Assistants",
                column: "TransportUnitId",
                principalTable: "TransportUnits",
                principalColumn: "TransportUnitId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assistants_TransportUnits_TransportUnitId",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Jobs");

            migrationBuilder.AddForeignKey(
                name: "FK_Assistants_TransportUnits_TransportUnitId",
                table: "Assistants",
                column: "TransportUnitId",
                principalTable: "TransportUnits",
                principalColumn: "TransportUnitId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
