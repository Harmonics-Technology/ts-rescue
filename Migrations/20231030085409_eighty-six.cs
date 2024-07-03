using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class eightysix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "adminCanApproveExpense",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "adminCanApprovePayrolls",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "adminCanApproveTimesheet",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "adminCanViewInvoice",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "adminCanViewPayrolls",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "adminCanApproveExpense",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "adminCanApprovePayrolls",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "adminCanApproveTimesheet",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "adminCanViewInvoice",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "adminCanViewPayrolls",
                table: "controlSettings");
        }
    }
}
