using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationM7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalPurchasePrice",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "PurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPurchasePrice",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "PurchaseItems");
        }
    }
}
