using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftyseven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isCompleted",
                table: "projectTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "projectTaskId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectTaskAsignees_userId",
                table: "projectTaskAsignees",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "iX_projectSubTasks_projectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTasks_ProjectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTaskAsignees_users_userId",
                table: "projectTaskAsignees",
                column: "userId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTasks_ProjectTaskId",
                table: "projectSubTasks");

            migrationBuilder.DropForeignKey(
                name: "fK_projectTaskAsignees_users_userId",
                table: "projectTaskAsignees");

            migrationBuilder.DropIndex(
                name: "iX_projectTaskAsignees_userId",
                table: "projectTaskAsignees");

            migrationBuilder.DropIndex(
                name: "iX_projectSubTasks_projectTaskId",
                table: "projectSubTasks");

            migrationBuilder.DropColumn(
                name: "isCompleted",
                table: "projectTasks");

            migrationBuilder.DropColumn(
                name: "projectTaskId",
                table: "projectSubTasks");
        }
    }
}
