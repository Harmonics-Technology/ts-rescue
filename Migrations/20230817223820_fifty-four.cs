using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftyfour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_onboardingFees_onboardingFeeTypes_onboardingFeeTypeId",
                table: "onboardingFees");

            migrationBuilder.DropTable(
                name: "onboardingFeeTypes");

            migrationBuilder.DropIndex(
                name: "iX_onboardingFees_onboardingFeeTypeId",
                table: "onboardingFees");

            migrationBuilder.DropColumn(
                name: "onboardingFeeTypeId",
                table: "onboardingFees");

            migrationBuilder.AddColumn<string>(
                name: "onboardingFeeType",
                table: "onboardingFees",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onboardingFeeType",
                table: "onboardingFees");

            migrationBuilder.AddColumn<int>(
                name: "onboardingFeeTypeId",
                table: "onboardingFees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "onboardingFeeTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_onboardingFeeTypes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_onboardingFees_onboardingFeeTypeId",
                table: "onboardingFees",
                column: "onboardingFeeTypeId");

            migrationBuilder.AddForeignKey(
                name: "fK_onboardingFees_onboardingFeeTypes_onboardingFeeTypeId",
                table: "onboardingFees",
                column: "onboardingFeeTypeId",
                principalTable: "onboardingFeeTypes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
