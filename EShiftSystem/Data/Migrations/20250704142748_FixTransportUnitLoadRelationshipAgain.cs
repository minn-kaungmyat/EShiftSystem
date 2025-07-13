using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTransportUnitLoadRelationshipAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loads_TransportUnits_TransportUnitId1",
                table: "Loads");

            migrationBuilder.DropIndex(
                name: "IX_Loads_TransportUnitId1",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "TransportUnitId1",
                table: "Loads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransportUnitId1",
                table: "Loads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loads_TransportUnitId1",
                table: "Loads",
                column: "TransportUnitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_TransportUnits_TransportUnitId1",
                table: "Loads",
                column: "TransportUnitId1",
                principalTable: "TransportUnits",
                principalColumn: "TransportUnitId");
        }
    }
}
