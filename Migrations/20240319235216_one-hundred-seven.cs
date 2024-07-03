using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehundredseven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "employmentContractType",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "newPayrollStructureEnabled",
                table: "employeeInformation",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payrollProcessingType",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payrollStructure",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "rate",
                table: "employeeInformation",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "rateType",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "standardCanadianSystem",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "tax",
                table: "employeeInformation",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "taxType",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "timesheetFrequency",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "department",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "employmentContractType",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "newPayrollStructureEnabled",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "payrollProcessingType",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "payrollStructure",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "rate",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "rateType",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "standardCanadianSystem",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "tax",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "taxType",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "timesheetFrequency",
                table: "employeeInformation");
        }
    }
}
