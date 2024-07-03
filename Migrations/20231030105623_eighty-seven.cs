using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class eightyseven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "adminCanViewInvoice",
                table: "controlSettings",
                newName: "adminCanViewTeamMemberInvoice");

            migrationBuilder.AddColumn<bool>(
                name: "adminCanViewClientInvoice",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "adminCanViewPaymentPartnerInvoice",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "adminCanViewClientInvoice",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "adminCanViewPaymentPartnerInvoice",
                table: "controlSettings");

            migrationBuilder.RenameColumn(
                name: "adminCanViewTeamMemberInvoice",
                table: "controlSettings",
                newName: "adminCanViewInvoice");
        }
    }
}
