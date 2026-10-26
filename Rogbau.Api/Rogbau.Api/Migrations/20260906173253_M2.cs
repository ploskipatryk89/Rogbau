using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class M2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuanityTotal",
                table: "Equipments",
                newName: "QuantityTotal");

            migrationBuilder.RenameColumn(
                name: "QuanityAvailable",
                table: "Equipments",
                newName: "QuantityAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityTotal",
                table: "Equipments",
                newName: "QuanityTotal");

            migrationBuilder.RenameColumn(
                name: "QuantityAvailable",
                table: "Equipments",
                newName: "QuanityAvailable");
        }
    }
}
