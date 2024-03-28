using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixtythree : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
