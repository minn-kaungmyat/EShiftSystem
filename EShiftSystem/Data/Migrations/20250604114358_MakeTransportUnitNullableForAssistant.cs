using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeTransportUnitNullableForAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Jobs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Jobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TransportUnitId",
                table: "Assistants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "TransportUnitId",
                table: "Assistants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
