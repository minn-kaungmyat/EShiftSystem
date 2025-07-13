using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceLoadItemWithRealisticFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "LoadItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LoadItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialInstructions",
                table: "LoadItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "LoadItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "SpecialInstructions",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "LoadItems");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "LoadItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
