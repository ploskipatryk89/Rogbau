using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityTotal",
                table: "Equipments");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "EquipmentMovements",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovements_Number",
                table: "EquipmentMovements",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquipmentMovements_Number",
                table: "EquipmentMovements");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "EquipmentMovements");

            migrationBuilder.AddColumn<int>(
                name: "QuantityTotal",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
