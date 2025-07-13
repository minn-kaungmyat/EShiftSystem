using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNoteFromLoadItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "LoadItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "LoadItems",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "LoadItems",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "LoadItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
