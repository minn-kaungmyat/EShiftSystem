using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveLocationToJobLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "StartLocation",
                table: "Loads");

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Jobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DestinationLatitude",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationLongitude",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartLatitude",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartLocation",
                table: "Jobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartLongitude",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StartLatitude",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StartLocation",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StartLongitude",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Loads",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartLocation",
                table: "Loads",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
