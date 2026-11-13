using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRaceDayDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ENROLMENT_STATUS",
                table: "ENROLMENT");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ENROLMENT_STATUS",
                table: "ENROLMENT",
                sql: "[STATUS] IN ('Pending', 'Active', 'Cancelled', 'Completed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ENROLMENT_STATUS",
                table: "ENROLMENT");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ENROLMENT_STATUS",
                table: "ENROLMENT",
                sql: "[STATUS] IN ('Active', 'Cancelled', 'Completed')");
        }
    }
}
