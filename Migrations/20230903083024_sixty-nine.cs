using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixtynine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "subTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "taskId",
                table: "projectTimesheets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "subTaskId",
                table: "projectTimesheets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "taskId",
                table: "projectTimesheets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_subTaskId",
                table: "projectTimesheets",
                column: "subTaskId");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_taskId",
                table: "projectTimesheets",
                column: "taskId");

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
    }
}
