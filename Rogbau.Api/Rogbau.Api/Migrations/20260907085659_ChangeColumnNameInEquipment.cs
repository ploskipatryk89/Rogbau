using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnNameInEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Weightkg",
                table: "Equipments",
                newName: "WeightKg");

            migrationBuilder.RenameColumn(
                name: "QuanityScrapped",
                table: "Equipments",
                newName: "QuantityScrapped");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeightKg",
                table: "Equipments",
                newName: "Weightkg");

            migrationBuilder.RenameColumn(
                name: "QuantityScrapped",
                table: "Equipments",
                newName: "QuanityScrapped");
        }
    }
}
