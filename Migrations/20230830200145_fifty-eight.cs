using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftyeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTasks_ProjectTaskId",
                table: "projectSubTasks");

            migrationBuilder.DropColumn(
                name: "taskId",
                table: "projectSubTasks");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTasks_projectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTasks_projectTaskId",
                table: "projectSubTasks");

            migrationBuilder.AddColumn<Guid>(
                name: "taskId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTasks_ProjectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id");
        }
    }
}
