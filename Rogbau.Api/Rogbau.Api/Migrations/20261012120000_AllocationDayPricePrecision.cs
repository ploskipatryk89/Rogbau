using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261012120000_AllocationDayPricePrecision")]
public class AllocationDayPricePrecision : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "DayPrice", table: "SiteEquipmentAllocations", type: "decimal(18,4)",
            precision: 18, scale: 4, nullable: false,
            oldClrType: typeof(decimal), oldType: "decimal(18,2)", oldPrecision: 18, oldScale: 2);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "DayPrice", table: "SiteEquipmentAllocations", type: "decimal(18,2)",
            precision: 18, scale: 2, nullable: false,
            oldClrType: typeof(decimal), oldType: "decimal(18,4)", oldPrecision: 18, oldScale: 4);
    }
}
