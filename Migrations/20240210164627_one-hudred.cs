using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehudred : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "paymentMethod",
                table: "clientSubscriptionDetails");

            migrationBuilder.AddColumn<decimal>(
                name: "subscriptionPrice",
                table: "clientSubscriptionDetails",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subscriptionPrice",
                table: "clientSubscriptionDetails");

            migrationBuilder.AddColumn<string>(
                name: "paymentMethod",
                table: "clientSubscriptionDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
