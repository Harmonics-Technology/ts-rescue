using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class twentyfour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "hST",
                table: "invoices",
                type: "double",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hST",
                table: "invoices");
        }
    }
}
