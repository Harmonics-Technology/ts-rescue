using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class seventytwo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "projectTaskAsigneeId",
                table: "projectTimesheets",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_projectTaskAsigneeId",
                table: "projectTimesheets",
                column: "projectTaskAsigneeId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projectTaskAsignees_projectTaskAsigneeId",
                table: "projectTimesheets",
                column: "projectTaskAsigneeId",
                principalTable: "projectTaskAsignees",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projectTaskAsignees_projectTaskAsigneeId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_projectTaskAsigneeId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "projectTaskAsigneeId",
                table: "projectTimesheets");
        }
    }
}
