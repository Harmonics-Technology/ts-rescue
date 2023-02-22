using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class nine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "createdByUserId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "paymentSchedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    cycle = table.Column<int>(type: "int", nullable: false),
                    weekDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    lastWorkDayOfCycle = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    approvalDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    paymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_paymentSchedules", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_createdByUserId",
                table: "invoices",
                column: "createdByUserId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_users_createdByUserId",
                table: "invoices",
                column: "createdByUserId",
                principalTable: "Users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_users_createdByUserId",
                table: "invoices");

            migrationBuilder.DropTable(
                name: "paymentSchedules");

            migrationBuilder.DropIndex(
                name: "iX_invoices_createdByUserId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "createdByUserId",
                table: "invoices");
        }
    }
}
