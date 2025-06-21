using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShiftSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobNumberWithDataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the JobNumber column without unique constraint
            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "Jobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Step 2: Update existing jobs with proper JobNumbers
            migrationBuilder.Sql(@"
                WITH JobsWithRowNumber AS (
                    SELECT 
                        JobId,
                        CreatedAt,
                        ROW_NUMBER() OVER (
                            PARTITION BY YEAR(CreatedAt), MONTH(CreatedAt) 
                            ORDER BY CreatedAt, JobId
                        ) AS RowNum,
                        YEAR(CreatedAt) AS JobYear,
                        MONTH(CreatedAt) AS JobMonth
                    FROM Jobs
                )
                UPDATE Jobs 
                SET JobNumber = 'JOB-' + 
                    CAST(j.JobYear AS VARCHAR(4)) + '-' + 
                    RIGHT('00' + CAST(j.JobMonth AS VARCHAR(2)), 2) + '-' + 
                    RIGHT('0000' + CAST(j.RowNum AS VARCHAR(4)), 4)
                FROM Jobs 
                INNER JOIN JobsWithRowNumber j ON Jobs.JobId = j.JobId
            ");

            // Step 3: Create the unique index after data is populated
            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobNumber",
                table: "Jobs",
                column: "JobNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobNumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "Jobs");
        }
    }
}
