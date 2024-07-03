using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class ninetynine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "clientSubscriptionStatus",
                table: "Users",
                newName: "isOrganizationProjectManager");

            migrationBuilder.AddColumn<Guid>(
                name: "projectManagementSettingId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "projectManagementSettings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    superAdminId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    adminProjectCreation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    pMProjectCreation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    allProjectCreation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminTaskCreation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    assignedPMTaskCreation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    projectMembersTaskCreation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    adminTaskViewing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    assignedPMTaskViewing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    projectMembersTaskViewing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    pMTaskEditing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    taskMembersTaskEditing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    projectMembersTaskEditing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    projectMembersTimesheetVisibility = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    taskMembersTimesheetVisibility = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_projectManagementSettings", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projectManagementSettings");

            migrationBuilder.DropColumn(
                name: "projectManagementSettingId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "isOrganizationProjectManager",
                table: "Users",
                newName: "clientSubscriptionStatus");
        }
    }
}
