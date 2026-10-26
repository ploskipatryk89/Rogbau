using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class M6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equipments_Name",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "CreatetAt",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Suppliers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "SubCategories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Sites",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "SiteEquipments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Recipients",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Purchases",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "PurchaseItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Notifications",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Equipments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdateedAt",
                table: "Categories",
                newName: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_Name",
                table: "Equipments",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equipments_Name",
                table: "Equipments");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Suppliers",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "SubCategories",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Sites",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "SiteEquipments",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Recipients",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Purchases",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PurchaseItems",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Notifications",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Equipments",
                newName: "UpdateedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Categories",
                newName: "UpdateedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatetAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_Name",
                table: "Equipments",
                column: "Name",
                unique: true);
        }
    }
}
