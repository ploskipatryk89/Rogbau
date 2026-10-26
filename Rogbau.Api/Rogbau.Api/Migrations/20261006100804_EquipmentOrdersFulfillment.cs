using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentOrdersFulfillment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "EquipmentMovements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovements_OrderId",
                table: "EquipmentMovements",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentMovements_Orders_OrderId",
                table: "EquipmentMovements",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentMovements_Orders_OrderId",
                table: "EquipmentMovements");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentMovements_OrderId",
                table: "EquipmentMovements");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "EquipmentMovements");
        }
    }
}
