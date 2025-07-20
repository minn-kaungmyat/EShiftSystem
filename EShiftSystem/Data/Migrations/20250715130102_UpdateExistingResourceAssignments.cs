using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingResourceAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing lorries that are assigned to transport units
            migrationBuilder.Sql(@"
                UPDATE Lorries 
                SET IsAssigned = 1 
                WHERE LorryId IN (
                    SELECT DISTINCT LorryId 
                    FROM TransportUnits 
                    WHERE LorryId IS NOT NULL
                )");

            // Update existing drivers that are assigned to transport units
            migrationBuilder.Sql(@"
                UPDATE Drivers 
                SET IsAssigned = 1 
                WHERE DriverId IN (
                    SELECT DISTINCT DriverId 
                    FROM TransportUnits 
                    WHERE DriverId IS NOT NULL
                )");

            // Update existing containers that are assigned to transport units
            migrationBuilder.Sql(@"
                UPDATE Containers 
                SET IsAssigned = 1 
                WHERE ContainerId IN (
                    SELECT DISTINCT ContainerId 
                    FROM TransportUnits 
                    WHERE ContainerId IS NOT NULL
                )");

            // Update existing assistants that are assigned to transport units
            migrationBuilder.Sql(@"
                UPDATE Assistants 
                SET IsAssigned = 1 
                WHERE TransportUnitId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reset all IsAssigned flags to false
            migrationBuilder.Sql("UPDATE Lorries SET IsAssigned = 0");
            migrationBuilder.Sql("UPDATE Drivers SET IsAssigned = 0");
            migrationBuilder.Sql("UPDATE Containers SET IsAssigned = 0");
            migrationBuilder.Sql("UPDATE Assistants SET IsAssigned = 0");
        }
    }
}
