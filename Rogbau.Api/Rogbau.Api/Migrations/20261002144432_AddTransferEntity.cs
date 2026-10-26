using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FromConstructionSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToConstructionSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentMovements_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentMovements_Sites_FromConstructionSiteId",
                        column: x => x.FromConstructionSiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentMovements_Sites_ToConstructionSiteId",
                        column: x => x.ToConstructionSiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentMovementItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentMovementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ScrappedQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentMovementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentMovementItems_EquipmentMovements_EquipmentMovementId",
                        column: x => x.EquipmentMovementId,
                        principalTable: "EquipmentMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentMovementItems_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovementItems_EquipmentId",
                table: "EquipmentMovementItems",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovementItems_EquipmentMovementId",
                table: "EquipmentMovementItems",
                column: "EquipmentMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovements_AddedByUserId",
                table: "EquipmentMovements",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovements_FromConstructionSiteId",
                table: "EquipmentMovements",
                column: "FromConstructionSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMovements_ToConstructionSiteId",
                table: "EquipmentMovements",
                column: "ToConstructionSiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentMovementItems");

            migrationBuilder.DropTable(
                name: "EquipmentMovements");
        }
    }
}
