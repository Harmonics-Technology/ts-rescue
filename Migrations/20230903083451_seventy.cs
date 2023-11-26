using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class seventy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "projectSubTaskId",
                table: "projectTimesheets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "projectTaskId",
                table: "projectTimesheets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_projectSubTaskId",
                table: "projectTimesheets",
                column: "projectSubTaskId");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_projectTaskId",
                table: "projectTimesheets",
                column: "projectTaskId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projectSubTasks_projectSubTaskId",
                table: "projectTimesheets",
                column: "projectSubTaskId",
                principalTable: "projectSubTasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projectTasks_projectTaskId",
                table: "projectTimesheets",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projectSubTasks_projectSubTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projectTasks_projectTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_projectSubTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_projectTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "projectSubTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "projectTaskId",
                table: "projectTimesheets");
        }
    }
}
