using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixtyfour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_assigneeId",
                table: "projectSubTasks");

            migrationBuilder.DropIndex(
                name: "iX_projectSubTasks_assigneeId",
                table: "projectSubTasks");

            migrationBuilder.DropColumn(
                name: "assigneeId",
                table: "projectSubTasks");

            migrationBuilder.AddColumn<Guid>(
                name: "projectTaskAsigneeId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectSubTasks_projectTaskAsigneeId",
                table: "projectSubTasks",
                column: "projectTaskAsigneeId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_ProjectTaskAsigneeId",
                table: "projectSubTasks",
                column: "projectTaskAsigneeId",
                principalTable: "projectTaskAsignees",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_ProjectTaskAsigneeId",
                table: "projectSubTasks");

            migrationBuilder.DropIndex(
                name: "iX_projectSubTasks_projectTaskAsigneeId",
                table: "projectSubTasks");

            migrationBuilder.DropColumn(
                name: "projectTaskAsigneeId",
                table: "projectSubTasks");

            migrationBuilder.AddColumn<Guid>(
                name: "assigneeId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectSubTasks_assigneeId",
                table: "projectSubTasks",
                column: "assigneeId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_assigneeId",
                table: "projectSubTasks",
                column: "assigneeId",
                principalTable: "projectTaskAsignees",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
