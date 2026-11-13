using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    USER_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FIRSTNAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LASTNAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EMAIL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "nvarchar(225)", maxLength: 225, nullable: false),
                    ROLE = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.USER_ID);
                    table.CheckConstraint("CK_USER_ROLE", "[ROLE] IN ('Participant', 'Organiser')");
                });

            migrationBuilder.CreateTable(
                name: "ORGANISER",
                columns: table => new
                {
                    ORGANISER_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(type: "int", nullable: false),
                    ORGANISER_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CONTACT_NAME = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORGANISER", x => x.ORGANISER_ID);
                    table.ForeignKey(
                        name: "FK_ORGANISER_USER_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PARTICIPANT",
                columns: table => new
                {
                    PARTICIPANT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(type: "int", nullable: false),
                    DATE_OF_BIRTH = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GENDER = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EMERGENCY_CONTACT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARTICIPANT", x => x.PARTICIPANT_ID);
                    table.ForeignKey(
                        name: "FK_PARTICIPANT_USER_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EVENT",
                columns: table => new
                {
                    EVENT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ORGONISER_ID = table.Column<int>(type: "int", nullable: false),
                    EVENT_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EVENT_DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LOCATION = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    STATUS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT", x => x.EVENT_ID);
                    table.ForeignKey(
                        name: "FK_EVENT_ORGANISER_ORGONISER_ID",
                        column: x => x.ORGONISER_ID,
                        principalTable: "ORGANISER",
                        principalColumn: "ORGANISER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CATEGORY",
                columns: table => new
                {
                    CATEGORY_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EVENT_ID = table.Column<int>(type: "int", nullable: false),
                    CATEGORY_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DISTANCE = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    ENTRY_FEE = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORY", x => x.CATEGORY_ID);
                    table.CheckConstraint("CK_CATEGORY_DISTANCE", "[DISTANCE] > 0");
                    table.CheckConstraint("CK_CATEGORY_ENTRY_FEE", "[ENTRY_FEE] >= 0");
                    table.ForeignKey(
                        name: "FK_CATEGORY_EVENT_EVENT_ID",
                        column: x => x.EVENT_ID,
                        principalTable: "EVENT",
                        principalColumn: "EVENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ENROLMENT",
                columns: table => new
                {
                    ENROLMENT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PARTICIPANT_ID = table.Column<int>(type: "int", nullable: false),
                    CATEGORY_ID = table.Column<int>(type: "int", nullable: false),
                    ENROLMENT_DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    STATUS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENROLMENT", x => x.ENROLMENT_ID);
                    table.CheckConstraint("CK_ENROLMENT_STATUS", "[STATUS] IN ('Active', 'Cancelled', 'Completed')");
                    table.ForeignKey(
                        name: "FK_ENROLMENT_CATEGORY_CATEGORY_ID",
                        column: x => x.CATEGORY_ID,
                        principalTable: "CATEGORY",
                        principalColumn: "CATEGORY_ID");
                    table.ForeignKey(
                        name: "FK_ENROLMENT_PARTICIPANT_PARTICIPANT_ID",
                        column: x => x.PARTICIPANT_ID,
                        principalTable: "PARTICIPANT",
                        principalColumn: "PARTICIPANT_ID");
                });

            migrationBuilder.CreateTable(
                name: "RESULT",
                columns: table => new
                {
                    RESULT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ENROLMENT_ID = table.Column<int>(type: "int", nullable: false),
                    FINISH_TIME = table.Column<TimeSpan>(type: "time", nullable: true),
                    POSITION = table.Column<int>(type: "int", nullable: true),
                    RESULT_STATUS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESULT", x => x.RESULT_ID);
                    table.CheckConstraint("CK_RESULT_POSITION", "[POSITION] IS NULL OR [POSITION] > 0");
                    table.CheckConstraint("CK_RESULT_STATUS", "[RESULT_STATUS] IN ('Pending', 'Finished', 'Did Not Finish', 'Disqualified')");
                    table.ForeignKey(
                        name: "FK_RESULT_ENROLMENT_ENROLMENT_ID",
                        column: x => x.ENROLMENT_ID,
                        principalTable: "ENROLMENT",
                        principalColumn: "ENROLMENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORY_EVENT_ID",
                table: "CATEGORY",
                column: "EVENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ENROLMENT_CATEGORY_ID",
                table: "ENROLMENT",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ENROLMENT_PARTICIPANT_ID_CATEGORY_ID",
                table: "ENROLMENT",
                columns: new[] { "PARTICIPANT_ID", "CATEGORY_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_ORGONISER_ID",
                table: "EVENT",
                column: "ORGONISER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORGANISER_USER_ID",
                table: "ORGANISER",
                column: "USER_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PARTICIPANT_USER_ID",
                table: "PARTICIPANT",
                column: "USER_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RESULT_ENROLMENT_ID",
                table: "RESULT",
                column: "ENROLMENT_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_EMAIL",
                table: "USER",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RESULT");

            migrationBuilder.DropTable(
                name: "ENROLMENT");

            migrationBuilder.DropTable(
                name: "CATEGORY");

            migrationBuilder.DropTable(
                name: "PARTICIPANT");

            migrationBuilder.DropTable(
                name: "EVENT");

            migrationBuilder.DropTable(
                name: "ORGANISER");

            migrationBuilder.DropTable(
                name: "USER");
        }
    }
}
