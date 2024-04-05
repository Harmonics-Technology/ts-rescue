using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehundredeleven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "employmentContractType",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "paymentProcessingFee",
                table: "userDrafts",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "paymentProcessingFeeType",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payrollProcessingType",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payrollStructure",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "rate",
                table: "userDrafts",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rateType",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "standardCanadianSystem",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "tax",
                table: "userDrafts",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "taxType",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "timesheetFrequency",
                table: "userDrafts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "department",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "employmentContractType",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "paymentProcessingFee",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "paymentProcessingFeeType",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "payrollProcessingType",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "payrollStructure",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "rate",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "rateType",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "standardCanadianSystem",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "tax",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "taxType",
                table: "userDrafts");

            migrationBuilder.DropColumn(
                name: "timesheetFrequency",
                table: "userDrafts");
        }
    }
}
