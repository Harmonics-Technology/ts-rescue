using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftyone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "adminOBoardibg",
                table: "controlSettings",
                newName: "allowShiftSwapRequest");

            migrationBuilder.AlterColumn<string>(
                name: "start",
                table: "shiftTypes",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "end",
                table: "shiftTypes",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "superAdminId",
                table: "onboardingFees",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "adminOBoarding",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "allowIneligibleLeaveCode",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "allowShiftSwapApproval",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "superAdminId",
                table: "onboardingFees");

            migrationBuilder.DropColumn(
                name: "adminOBoarding",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "allowIneligibleLeaveCode",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "allowShiftSwapApproval",
                table: "controlSettings");

            migrationBuilder.RenameColumn(
                name: "allowShiftSwapRequest",
                table: "controlSettings",
                newName: "adminOBoardibg");

            migrationBuilder.AlterColumn<int>(
                name: "start",
                table: "shiftTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "end",
                table: "shiftTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
