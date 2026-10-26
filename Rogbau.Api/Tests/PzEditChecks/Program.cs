using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rogbau.Api.Features.EquipmentTransfer.Commands.EditPz;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

var source = new ConfigurationBuilder().SetBasePath(Path.GetFullPath(args[0])).AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true).Build();
var connection = new SqlConnectionStringBuilder(source.GetConnectionString("DefaultConnection"));
var database = "Rogbau_PzEditChecks_" + Guid.NewGuid().ToString("N");
connection.InitialCatalog = database;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
ApplicationDbContext NewDb() => new(options);
void Check(bool condition, string label) { if (!condition) throw new Exception("FAIL: " + label); Console.WriteLine("PASS: " + label); }
var category = new Category { Name = "Test" };
var equipment = new Equipment { Name = "A", Unit = "szt", Category = category, QuantityAvailable = 5, QuantityScrapped = 2, InitialQuantity = 9, DayPrice = 2 };
var site = new Site { Name = "A" };
var siteB = new Site { Name = "B" };
var user = new ApplicationUser { UserName = "pz-test" };
var movement = new EquipmentMovement { Type = EquipmentMovementType.PZ, Status = EquipmentMovementStatus.Completed,
    FromConstructionSite = site, AddedBy = user, Items = new List<EquipmentMovementItem> {
        new EquipmentMovementItem { Equipment = equipment, Quantity = 8, ScrappedQuantity = 1, ReceivedQuantity = 7, ReceivedScrappedQuantity = 2 } } };
var created = false;
try
{
    await using (var db = NewDb())
    {
        await db.Database.EnsureCreatedAsync(); created = true;
        db.AddRange(movement, siteB);
        db.SiteEquipments.Add(new SiteEquipment { Site = site, Equipment = equipment, Quantity = 2 });
        db.SiteEquipments.Add(new SiteEquipment { Site = siteB, Equipment = equipment, Quantity = 10 });
        db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = site, Equipment = equipment, Quantity = 2, DateFrom = DateTime.UtcNow.AddDays(-5), DayPrice = 2 });
        db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = siteB, Equipment = equipment, Quantity = 10, DateFrom = DateTime.UtcNow.AddDays(-5), DayPrice = 2 });
        await db.SaveChangesAsync();
    }
    EditPzItemDto Item(int reported, int received, int scrap) => new EditPzItemDto { EquipmentId = equipment.Id, Quantity = reported, ScrappedQuantity = 1, ReceivedQuantity = received, ReceivedScrappedQuantity = scrap };
    async Task Edit(Guid sourceSite, EditPzItemDto item, bool admin = true)
    {
        await using var db = NewDb();
        await new EditPzCommandHandler(db, new TestUser(admin)).Handle(new EditPzCommand {
            Id = movement.Id, FromConstructionSiteId = sourceSite, Note = "Edited", Items = new List<EditPzItemDto> { item } }, default);
    }
    await Edit(site.Id, Item(9, 7, 2));
    await using (var db = NewDb())
    {
        Check((await db.Equipments.FindAsync(equipment.Id))!.QuantityAvailable == 5 && await db.SiteEquipmentAllocations.CountAsync() == 2,
            "reported quantity change does not affect completed receipt stock or allocations");
    }
    await Edit(site.Id, Item(9, 6, 1));
    await using (var db = NewDb())
    {
        var e = await db.Equipments.FindAsync(equipment.Id);
        var stock = await db.SiteEquipments.SingleAsync(x => x.SiteId == site.Id);
        Check(e!.QuantityAvailable == 5 && e.QuantityScrapped == 1 && stock.Quantity == 3,
            "received quantity and scrap correction balance warehouse and construction stock");
        Check(await db.SiteEquipmentAllocations.Where(x => x.SiteId == site.Id && x.DateTo == null).SumAsync(x => x.Quantity) == 3,
            "active allocations match corrected site stock");
    }
    await Edit(siteB.Id, Item(9, 6, 1));
    await using (var db = NewDb())
    {
        Check((await db.SiteEquipments.SingleAsync(x => x.SiteId == site.Id)).Quantity == 9 &&
            (await db.SiteEquipments.SingleAsync(x => x.SiteId == siteB.Id)).Quantity == 4,
            "site replacement restores old site and deducts new site");
    }
    var denied = false;
    try { await Edit(siteB.Id, Item(9, 6, 1), false); } catch (UnauthorizedAccessException) { denied = true; }
    Check(denied, "non-admin cannot edit PZ");
    var invalid = false;
    try { await Edit(siteB.Id, Item(20, 20, 1)); } catch (InvalidOperationException) { invalid = true; }
    Check(invalid, "impossible site stock correction is rejected");
    await using (var db = NewDb())
    {
        Check((await db.Equipments.FindAsync(equipment.Id))!.QuantityAvailable == 5 &&
            (await db.EquipmentMovements.Include(x => x.Items).SingleAsync(x => x.Id == movement.Id)).Items.Single().ReceivedQuantity == 6,
            "failed edit rolls back warehouse and document");
        var transit = new EquipmentMovement { Number = "PZ/TEST/TRANSIT", Type = EquipmentMovementType.PZ, Status = EquipmentMovementStatus.InTransit,
            FromConstructionSiteId = siteB.Id, AddedByUserId = user.Id,
            Items = new List<EquipmentMovementItem> { new EquipmentMovementItem { EquipmentId = equipment.Id, Quantity = 3 } } };
        db.Add(transit);
        (await db.Equipments.FindAsync(equipment.Id))!.QuantityInTransit = 3;
        await db.SaveChangesAsync();
        await new EditPzCommandHandler(db, new TestUser(true)).Handle(new EditPzCommand { Id = transit.Id, FromConstructionSiteId = siteB.Id,
            Items = new List<EditPzItemDto> { new EditPzItemDto { EquipmentId = equipment.Id, Quantity = 4 } } }, default);
    }
    await using (var db = NewDb())
    {
        Check((await db.Equipments.FindAsync(equipment.Id))!.QuantityInTransit == 4 &&
            (await db.SiteEquipments.SingleAsync(x => x.SiteId == siteB.Id)).Quantity == 3,
            "in-transit edit adjusts transit and site without warehouse receipt");
    }
    var replacement = new Equipment { Name = "B", Unit = "szt", CategoryId = category.Id, DayPrice = 3 };
    await using (var db = NewDb())
    {
        db.Add(replacement);
        db.SiteEquipments.Add(new SiteEquipment { SiteId = siteB.Id, Equipment = replacement, Quantity = 10 });
        db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { SiteId = siteB.Id, Equipment = replacement, Quantity = 10,
            DateFrom = DateTime.UtcNow.AddDays(-2), DayPrice = 3 });
        await db.SaveChangesAsync();
        await new EditPzCommandHandler(db, new TestUser(true)).Handle(new EditPzCommand { Id = movement.Id,
            FromConstructionSiteId = siteB.Id, Items = new List<EditPzItemDto> { new EditPzItemDto {
                EquipmentId = replacement.Id, Quantity = 6, ReceivedQuantity = 6, ReceivedScrappedQuantity = 1 } } }, default);
    }
    await using (var db = NewDb())
    {
        var oldEquipment = await db.Equipments.FindAsync(equipment.Id);
        var newEquipment = await db.Equipments.FindAsync(replacement.Id);
        Check(oldEquipment!.QuantityAvailable == 0 && oldEquipment.QuantityScrapped == 0 &&
            newEquipment!.QuantityAvailable == 5 && newEquipment.QuantityScrapped == 1,
            "equipment replacement reverses old warehouse receipt and records the new one");
        Check((await db.SiteEquipments.SingleAsync(x => x.SiteId == siteB.Id && x.EquipmentId == equipment.Id)).Quantity == 9 &&
            (await db.SiteEquipments.SingleAsync(x => x.SiteId == siteB.Id && x.EquipmentId == replacement.Id)).Quantity == 4,
            "equipment replacement balances both site stocks");
    }
}
finally
{
    if (created) { await using var db = NewDb(); if (new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog != database) throw new Exception("Unexpected database"); await db.Database.EnsureDeletedAsync(); Console.WriteLine("Temporary test database removed."); }
}
class TestUser : ICurrentUserService
{
    private readonly bool admin;
    public TestUser(bool admin) { this.admin = admin; }
    public string? UserId => "test";
    public string? Email => null;
    public bool IsAuthenticated => true;
    public bool IsInRole(string role) => admin && role == "Admin";
    public IEnumerable<string> GetRoles() => admin ? new[] { "Admin" } : new[] { "Kierownik Budowy" };
}
