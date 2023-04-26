using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class thirtyseven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "isSwapped",
                table: "shifts");

            migrationBuilder.DropColumn(
                name: "shiftSwappedId",
                table: "shifts");

            migrationBuilder.DropColumn(
                name: "swapStatusId",
                table: "shifts");

            migrationBuilder.RenameColumn(
                name: "shiftToSwapId",
                table: "shifts",
                newName: "swapId");

            migrationBuilder.CreateTable(
                name: "swaps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    swapperId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    swapeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    shiftId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    shiftId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    shiftToSwapId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    shiftToSwapId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    isApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_swaps", x => x.id);
                    table.ForeignKey(
                        name: "fK_swaps_shifts_shiftId1",
                        column: x => x.shiftId1,
                        principalTable: "shifts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_swaps_shifts_shiftToSwapId1",
                        column: x => x.shiftToSwapId1,
                        principalTable: "shifts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_swaps_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_swaps_users_swapeeId",
                        column: x => x.swapeeId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_swaps_users_swapperId",
                        column: x => x.swapperId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_shiftId1",
                table: "swaps",
                column: "shiftId1");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_shiftToSwapId1",
                table: "swaps",
                column: "shiftToSwapId1");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_statusId",
                table: "swaps",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_swapeeId",
                table: "swaps",
                column: "swapeeId");

            migrationBuilder.CreateIndex(
                name: "iX_swaps_swapperId",
                table: "swaps",
                column: "swapperId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "swaps");

            migrationBuilder.RenameColumn(
                name: "swapId",
                table: "shifts",
                newName: "shiftToSwapId");

            migrationBuilder.AddColumn<bool>(
                name: "isSwapped",
                table: "shifts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "shiftSwappedId",
                table: "shifts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "swapStatusId",
                table: "shifts",
                type: "int",
                nullable: true);

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
    }
}
