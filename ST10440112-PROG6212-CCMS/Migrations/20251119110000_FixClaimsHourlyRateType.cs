using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10440112_PROG6212_CCMS.Migrations
{
    /// <inheritdoc />
    public partial class FixClaimsHourlyRateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Change Claims.HourlyRate from int to decimal(10,2)
            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                table: "Claims",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to int (for rollback purposes)
            migrationBuilder.AlterColumn<int>(
                name: "HourlyRate",
                table: "Claims",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);
        }
    }
}
