using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "trackedByHours",
                table: "projectTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "hoursLogged",
                table: "projectTaskAsignees",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "isCompleted",
                table: "projectSubTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_subTaskId",
                table: "projectTimesheets",
                column: "subTaskId");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_taskId",
                table: "projectTimesheets",
                column: "taskId");

            migrationBuilder.CreateIndex(
                name: "iX_projectSubTasks_assigneeId",
                table: "projectSubTasks",
                column: "assigneeId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_users_assigneeId",
                table: "projectSubTasks",
                column: "assigneeId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projectSubTasks_subTaskId",
                table: "projectTimesheets",
                column: "subTaskId",
                principalTable: "projectSubTasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projectTasks_taskId",
                table: "projectTimesheets",
                column: "taskId",
                principalTable: "projectTasks",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_users_assigneeId",
                table: "projectSubTasks");

            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projectSubTasks_subTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projectTasks_taskId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_subTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_taskId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectSubTasks_assigneeId",
                table: "projectSubTasks");

            migrationBuilder.DropColumn(
                name: "trackedByHours",
                table: "projectTasks");

            migrationBuilder.DropColumn(
                name: "hoursLogged",
                table: "projectTaskAsignees");

            migrationBuilder.DropColumn(
                name: "isCompleted",
                table: "projectSubTasks");
        }
    }
}
