using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10440112_PROG6212_CCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyRatePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("0e6068e4-3d39-402c-b55e-67dcbe6c5b1a"));

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("951e5b0f-81c9-4988-ae54-ec48f7910d98"));

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "LecturerId",
                keyValue: new Guid("99d26d38-81c8-45c8-94a5-2953dcaec8f0"));

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Lecturers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Lecturers");

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "MajorDegree" },
                values: new object[] { new Guid("0e6068e4-3d39-402c-b55e-67dcbe6c5b1a"), "ebrahim.jacobs@newlands.ac.za", "Ebrahim Jacobs", "ProgrammeCoordinator", "Computer Science" });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "Faculty" },
                values: new object[] { new Guid("951e5b0f-81c9-4988-ae54-ec48f7910d98"), "janet.duplessis@newlands.ac.za", "Janet Du Plessis", "AcademicManager", "Faculty of Science" });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "LecturerId", "Department", "Email", "Name" },
                values: new object[] { new Guid("99d26d38-81c8-45c8-94a5-2953dcaec8f0"), "Faculty of Science", "michael.jones@newlands.ac.za", "Michael Jones" });
        }
    }
}
