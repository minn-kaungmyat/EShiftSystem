using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixExistingCustomersActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all existing customers to be active
            migrationBuilder.Sql("UPDATE Customers SET IsActive = 1 WHERE IsActive = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert all customers to inactive (original state after the first migration)
            migrationBuilder.Sql("UPDATE Customers SET IsActive = 0");
        }
    }
}
