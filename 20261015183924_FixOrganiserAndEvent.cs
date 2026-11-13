using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class FixOrganiserAndEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EVENT_ORGANISER_ORGONISER_ID",
                table: "EVENT");

            migrationBuilder.RenameColumn(
                name: "ORGANISER_NAME",
                table: "ORGANISER",
                newName: "ORGANISATION_NAME");

            migrationBuilder.RenameColumn(
                name: "CONTACT_NAME",
                table: "ORGANISER",
                newName: "CONTACT_NUMBER");

            migrationBuilder.RenameColumn(
                name: "ORGONISER_ID",
                table: "EVENT",
                newName: "ORGANISER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_EVENT_ORGONISER_ID",
                table: "EVENT",
                newName: "IX_EVENT_ORGANISER_ID");

            migrationBuilder.AddColumn<decimal>(
                name: "DISTANCE",
                table: "EVENT",
                type: "decimal(6,2)",
                nullable: false,
               defaultValue: 5m);

            migrationBuilder.AddColumn<string>(
                name: "EVENT_TYPE",
                table: "EVENT",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Run");

            migrationBuilder.AddForeignKey(
                name: "FK_EVENT_ORGANISER_ORGANISER_ID",
                table: "EVENT",
                column: "ORGANISER_ID",
                principalTable: "ORGANISER",
                principalColumn: "ORGANISER_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EVENT_ORGANISER_ORGANISER_ID",
                table: "EVENT");

            migrationBuilder.DropColumn(
                name: "DISTANCE",
                table: "EVENT");

            migrationBuilder.DropColumn(
                name: "EVENT_TYPE",
                table: "EVENT");

            migrationBuilder.RenameColumn(
                name: "ORGANISATION_NAME",
                table: "ORGANISER",
                newName: "ORGANISER_NAME");

            migrationBuilder.RenameColumn(
                name: "CONTACT_NUMBER",
                table: "ORGANISER",
                newName: "CONTACT_NAME");

            migrationBuilder.RenameColumn(
                name: "ORGANISER_ID",
                table: "EVENT",
                newName: "ORGONISER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_EVENT_ORGANISER_ID",
                table: "EVENT",
                newName: "IX_EVENT_ORGONISER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EVENT_ORGANISER_ORGONISER_ID",
                table: "EVENT",
                column: "ORGONISER_ID",
                principalTable: "ORGANISER",
                principalColumn: "ORGANISER_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
