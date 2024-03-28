using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class seventyfour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amountEarned",
                table: "projectTimesheets",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "budget",
                table: "projectTaskAsignees",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "budgetSpent",
                table: "projectTaskAsignees",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "budgetSpent",
                table: "projects",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "hoursSpent",
                table: "projects",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_projectId",
                table: "projectTimesheets",
                column: "projectId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projects_projectId",
                table: "projectTimesheets",
                column: "projectId",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projects_projectId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_projectId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "amountEarned",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "budget",
                table: "projectTaskAsignees");

            migrationBuilder.DropColumn(
                name: "budgetSpent",
                table: "projectTaskAsignees");

            migrationBuilder.DropColumn(
                name: "budgetSpent",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "hoursSpent",
                table: "projects");
        }
    }
}
