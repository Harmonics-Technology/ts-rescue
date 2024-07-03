using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehundrednineteen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "createdByUserId",
                table: "projectTasks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectTasks_createdByUserId",
                table: "projectTasks",
                column: "createdByUserId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTasks_users_createdByUserId",
                table: "projectTasks",
                column: "createdByUserId",
                principalTable: "Users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTasks_users_createdByUserId",
                table: "projectTasks");

            migrationBuilder.DropIndex(
                name: "iX_projectTasks_createdByUserId",
                table: "projectTasks");

            migrationBuilder.DropColumn(
                name: "createdByUserId",
                table: "projectTasks");
        }
    }
}
