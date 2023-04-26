using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class twentynine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isEligibleForLeave",
                table: "employeeInformation",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numberOfDaysEligible",
                table: "employeeInformation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numberOfHoursEligible",
                table: "employeeInformation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "timeSheetGenerationStartDate",
                table: "employeeInformation",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isEligibleForLeave",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "numberOfDaysEligible",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "numberOfHoursEligible",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "timeSheetGenerationStartDate",
                table: "employeeInformation");
        }
    }
}
