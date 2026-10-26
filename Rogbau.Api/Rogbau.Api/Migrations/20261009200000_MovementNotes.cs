using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261009200000_MovementNotes")]
public class MovementNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Note", table: "EquipmentMovements", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Note", table: "EquipmentMovements");
    }
}
