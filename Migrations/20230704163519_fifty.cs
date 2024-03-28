using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fifty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "controlSettingId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "leaveConfigurationId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "superAdminId",
                table: "leaveTypes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "controlSettings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    superAdminId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    twoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminOBoardibg = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminContractManagement = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminLeaveManagement = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminShiftManagement = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminReport = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminExpenseTypeAndHST = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_controlSettings", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "leaveConfigurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    superAdminId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    eligibleLeaveDays = table.Column<int>(type: "int", nullable: false),
                    isStandardEligibleDays = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_leaveConfigurations", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shiftTypes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    superAdminId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duration = table.Column<int>(type: "int", nullable: false),
                    color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start = table.Column<int>(type: "int", nullable: false),
                    end = table.Column<int>(type: "int", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_shiftTypes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "controlSettings");

            migrationBuilder.DropTable(
                name: "leaveConfigurations");

            migrationBuilder.DropTable(
                name: "shiftTypes");

            migrationBuilder.DropColumn(
                name: "controlSettingId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "leaveConfigurationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "superAdminId",
                table: "leaveTypes");
        }
    }
}
