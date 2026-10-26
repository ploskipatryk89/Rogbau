using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogbau.Api.Migrations
{
    /// <inheritdoc />
    public partial class M10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteUser_AspNetUsers_UserId",
                table: "SiteUser");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUser_Sites_SiteId",
                table: "SiteUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteUser",
                table: "SiteUser");

            migrationBuilder.RenameTable(
                name: "SiteUser",
                newName: "SiteUsers");

            migrationBuilder.RenameIndex(
                name: "IX_SiteUser_UserId",
                table: "SiteUsers",
                newName: "IX_SiteUsers_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteUsers",
                table: "SiteUsers",
                columns: new[] { "SiteId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUsers_AspNetUsers_UserId",
                table: "SiteUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUsers_Sites_SiteId",
                table: "SiteUsers",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteUsers_AspNetUsers_UserId",
                table: "SiteUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUsers_Sites_SiteId",
                table: "SiteUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteUsers",
                table: "SiteUsers");

            migrationBuilder.RenameTable(
                name: "SiteUsers",
                newName: "SiteUser");

            migrationBuilder.RenameIndex(
                name: "IX_SiteUsers_UserId",
                table: "SiteUser",
                newName: "IX_SiteUser_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteUser",
                table: "SiteUser",
                columns: new[] { "SiteId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUser_AspNetUsers_UserId",
                table: "SiteUser",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUser_Sites_SiteId",
                table: "SiteUser",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
