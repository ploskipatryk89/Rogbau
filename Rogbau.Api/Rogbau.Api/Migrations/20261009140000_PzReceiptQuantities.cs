using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261009140000_PzReceiptQuantities")]
    public class PzReceiptQuantities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "ReceivedQuantity", table: "EquipmentMovementItems",
                type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ReceivedScrappedQuantity", table: "EquipmentMovementItems",
                type: "int", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReceivedQuantity", table: "EquipmentMovementItems");
            migrationBuilder.DropColumn(name: "ReceivedScrappedQuantity", table: "EquipmentMovementItems");
        }
    }
}
