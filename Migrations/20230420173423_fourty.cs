using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "shiftId1",
                table: "swaps",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "shiftToSwapId1",
                table: "swaps",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_shiftId1",
                table: "swaps",
                column: "shiftId1");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_shiftToSwapId1",
                table: "swaps",
                column: "shiftToSwapId1");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_swapeeId",
                table: "swaps",
                column: "swapeeId");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_swapperId",
                table: "swaps",
                column: "swapperId");

            migrationBuilder.CreateIndex(
                name: "iX_shifts_swapId",
                table: "shifts",
                column: "swapId");

            migrationBuilder.AddForeignKey(
                name: "fK_shifts_swaps_swapId1",
                table: "shifts",
                column: "swapId",
                principalTable: "swaps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_swaps_shifts_shiftId1",
                table: "swaps",
                column: "shiftId1",
                principalTable: "shifts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_swaps_shifts_shiftToSwapId1",
                table: "swaps",
                column: "shiftToSwapId1",
                principalTable: "shifts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_swaps_users_swapeeId",
                table: "swaps",
                column: "swapeeId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fK_swaps_users_swapperId",
                table: "swaps",
                column: "swapperId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_shifts_swaps_swapId1",
                table: "shifts");

            migrationBuilder.DropForeignKey(
                name: "fK_swaps_shifts_shiftId1",
                table: "swaps");

            migrationBuilder.DropForeignKey(
                name: "fK_swaps_shifts_shiftToSwapId1",
                table: "swaps");

            migrationBuilder.DropForeignKey(
                name: "fK_swaps_users_swapeeId",
                table: "swaps");

            migrationBuilder.DropForeignKey(
                name: "fK_swaps_users_swapperId",
                table: "swaps");

            migrationBuilder.DropIndex(
                name: "iX_swaps_shiftId1",
                table: "swaps");

            migrationBuilder.DropIndex(
                name: "iX_swaps_shiftToSwapId1",
                table: "swaps");

            migrationBuilder.DropIndex(
                name: "iX_swaps_swapeeId",
                table: "swaps");

            migrationBuilder.DropIndex(
                name: "iX_swaps_swapperId",
                table: "swaps");

            migrationBuilder.DropIndex(
                name: "iX_shifts_swapId",
                table: "shifts");

            migrationBuilder.DropColumn(
                name: "shiftId1",
                table: "swaps");

            migrationBuilder.DropColumn(
                name: "shiftToSwapId1",
                table: "swaps");
        }
    }
}
