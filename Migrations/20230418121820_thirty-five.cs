using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class thirtyfive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "shiftToSwapId",
                table: "shifts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_shifts_shiftToSwapId",
                table: "shifts",
                column: "shiftToSwapId");

            migrationBuilder.CreateIndex(
                name: "iX_shifts_swapStatusId",
                table: "shifts",
                column: "swapStatusId");

            migrationBuilder.AddForeignKey(
                name: "fK_shifts_shifts_shiftToSwapId",
                table: "shifts",
                column: "shiftToSwapId",
                principalTable: "shifts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_shifts_statuses_swapStatusId",
                table: "shifts",
                column: "swapStatusId",
                principalTable: "statuses",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_shifts_shifts_shiftToSwapId",
                table: "shifts");

            migrationBuilder.DropForeignKey(
                name: "fK_shifts_statuses_swapStatusId",
                table: "shifts");

            migrationBuilder.DropIndex(
                name: "iX_shifts_shiftToSwapId",
                table: "shifts");

            migrationBuilder.DropIndex(
                name: "iX_shifts_swapStatusId",
                table: "shifts");

            migrationBuilder.DropColumn(
                name: "shiftToSwapId",
                table: "shifts");
        }
    }
}
