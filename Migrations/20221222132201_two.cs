using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class two : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_expenses_expenseTypes_ExpenseTypeId",
                table: "expenses");

            migrationBuilder.DropForeignKey(
                name: "fK_Users_Users_clientId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "iX_Users_clientId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "enponseTypeId",
                table: "expenses");

            migrationBuilder.AlterColumn<Guid>(
                name: "expenseTypeId",
                table: "expenses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_Users_clientId",
                table: "Users",
                column: "clientId");

            migrationBuilder.AddForeignKey(
                name: "fK_expenses_expenseTypes_expenseTypeId",
                table: "expenses",
                column: "expenseTypeId",
                principalTable: "expenseTypes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fK_Users_Users_clientId",
                table: "Users",
                column: "clientId",
                principalTable: "Users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_expenses_expenseTypes_expenseTypeId",
                table: "expenses");

            migrationBuilder.DropForeignKey(
                name: "fK_Users_Users_clientId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "iX_Users_clientId",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "expenseTypeId",
                table: "expenses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "enponseTypeId",
                table: "expenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "iX_Users_clientId",
                table: "Users",
                column: "clientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fK_expenses_expenseTypes_ExpenseTypeId",
                table: "expenses",
                column: "expenseTypeId",
                principalTable: "expenseTypes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_Users_Users_clientId1",
                table: "Users",
                column: "clientId",
                principalTable: "Users",
                principalColumn: "id");
        }
    }
}
