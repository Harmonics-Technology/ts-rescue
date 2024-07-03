using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehundrednine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "convertedAmount",
                table: "invoices",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "rateForConvertedIvoice",
                table: "invoices",
                type: "double",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "convertedAmount",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "rateForConvertedIvoice",
                table: "invoices");
        }
    }
}
