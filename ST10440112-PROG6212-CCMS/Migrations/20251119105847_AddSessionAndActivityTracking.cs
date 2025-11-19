using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10440112_PROG6212_CCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionAndActivityTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("a4de038f-0e2b-49fb-aea5-737ecd6b2b66"));

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("ca9a7596-7f0c-4d11-b820-7c157da0b0f0"));

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "LecturerId",
                keyValue: new Guid("ba922324-d07d-4cf6-a424-3d9821b31eb7"));

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.ActivityId);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.SessionId);
                });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "MajorDegree" },
                values: new object[] { new Guid("61f91108-6988-41b4-bb44-dea5ccce8361"), "ebrahim.jacobs@newlands.ac.za", "Ebrahim Jacobs", "ProgrammeCoordinator", "Computer Science" });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "Faculty" },
                values: new object[] { new Guid("e711d28a-b975-4517-b82a-f8b86316ea6e"), "janet.duplessis@newlands.ac.za", "Janet Du Plessis", "AcademicManager", "Faculty of Science" });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "LecturerId", "Department", "Email", "HourlyRate", "Name" },
                values: new object[] { new Guid("7ee42b7d-524f-4761-8723-c82cd69fc32b"), "Faculty of Science", "michael.jones@newlands.ac.za", 350m, "Michael Jones" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("61f91108-6988-41b4-bb44-dea5ccce8361"));

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("e711d28a-b975-4517-b82a-f8b86316ea6e"));

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "LecturerId",
                keyValue: new Guid("7ee42b7d-524f-4761-8723-c82cd69fc32b"));

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "MajorDegree" },
                values: new object[] { new Guid("a4de038f-0e2b-49fb-aea5-737ecd6b2b66"), "ebrahim.jacobs@newlands.ac.za", "Ebrahim Jacobs", "ProgrammeCoordinator", "Computer Science" });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "Faculty" },
                values: new object[] { new Guid("ca9a7596-7f0c-4d11-b820-7c157da0b0f0"), "janet.duplessis@newlands.ac.za", "Janet Du Plessis", "AcademicManager", "Faculty of Science" });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "LecturerId", "Department", "Email", "HourlyRate", "Name" },
                values: new object[] { new Guid("ba922324-d07d-4cf6-a424-3d9821b31eb7"), "Faculty of Science", "michael.jones@newlands.ac.za", 350m, "Michael Jones" });
        }
    }
}
