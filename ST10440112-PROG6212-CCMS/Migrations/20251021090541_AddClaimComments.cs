using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10440112_PROG6212_CCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("4d069670-e4ba-404a-adca-ea80d69acdc9"));

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("687f12ce-dcc8-4a89-9bbc-99ffe7571bab"));

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "LecturerId",
                keyValue: new Guid("0ec98110-6271-4678-a357-e1fdca5712b2"));

            migrationBuilder.CreateTable(
                name: "ClaimComments",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AuthorRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_ClaimComments_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "MajorDegree" },
                values: new object[] { new Guid("0c85a790-44e5-48cb-9e91-551a0478b49d"), "ebrahim.jacobs@newlands.ac.za", "Ebrahim Jacobs", "ProgrammeCoordinator", "Computer Science" });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "Faculty" },
                values: new object[] { new Guid("a4e6db9c-0bdc-413e-9fbe-5b0ce655abef"), "janet.duplessis@newlands.ac.za", "Janet Du Plessis", "AcademicManager", "Faculty of Science" });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "LecturerId", "Department", "Email", "Name" },
                values: new object[] { new Guid("b7e69ba8-d1e9-417c-b248-7c2f119e4130"), "Faculty of Science", "michael.jones@newlands.ac.za", "Michael Jones" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimComments_ClaimId",
                table: "ClaimComments",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimComments");

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("0c85a790-44e5-48cb-9e91-551a0478b49d"));

            migrationBuilder.DeleteData(
                table: "Admin",
                keyColumn: "AdminID",
                keyValue: new Guid("a4e6db9c-0bdc-413e-9fbe-5b0ce655abef"));

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "LecturerId",
                keyValue: new Guid("b7e69ba8-d1e9-417c-b248-7c2f119e4130"));

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "Faculty" },
                values: new object[] { new Guid("4d069670-e4ba-404a-adca-ea80d69acdc9"), "janet.duplessis@newlands.ac.za", "Janet Du Plessis", "AcademicManager", "Faculty of Science" });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminID", "AdminEmail", "AdminName", "Discriminator", "MajorDegree" },
                values: new object[] { new Guid("687f12ce-dcc8-4a89-9bbc-99ffe7571bab"), "ebrahim.jacobs@newlands.ac.za", "Ebrahim Jacobs", "ProgrammeCoordinator", "Computer Science" });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "LecturerId", "Department", "Email", "Name" },
                values: new object[] { new Guid("0ec98110-6271-4678-a357-e1fdca5712b2"), "Faculty of Science", "michael.jones@newlands.ac.za", "Michael Jones" });
        }
    }
}
