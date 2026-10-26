using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rogbau.Api.Features.EquipmentTransfer.Commands.EditWz;
using Rogbau.Api.Features.EquipmentTransfer.Commands.EditTransfer;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

var config = new ConfigurationBuilder().SetBasePath(Path.GetFullPath(args[0])).AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true).Build();
var connection = new SqlConnectionStringBuilder(config.GetConnectionString("DefaultConnection"));
string database = "Rogbau_MovementEditChecks_" + Guid.NewGuid().ToString("N");
connection.InitialCatalog = database;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
ApplicationDbContext NewDb() => new ApplicationDbContext(options);
int checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new Exception("FAIL: " + label);
    checks++;
    Console.WriteLine("PASS: " + label);
}
async Task<Fixture> Seed(EquipmentMovementType type, EquipmentMovementStatus status)
{
    var f = new Fixture();
    f.Movement.Type = type;
    f.Movement.Status = status;
    f.Movement.AddedBy = f.User;
    f.Movement.ToConstructionSite = f.B;
    if (type == EquipmentMovementType.Transfer) f.Movement.FromConstructionSite = f.A;
    f.Movement.Items.Add(new EquipmentMovementItem { Equipment = f.Equipment, Quantity = 5 });
    if (status == EquipmentMovementStatus.InTransit) f.Equipment.QuantityInTransit = 5;
    await using var db = NewDb();
    db.AddRange(f.Movement, f.A, f.C, f.Replacement);
    db.SiteEquipments.Add(new SiteEquipment { Site = f.A, Equipment = f.Equipment, Quantity = 10 });
    db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = f.A, Equipment = f.Equipment,
        Quantity = 10, DateFrom = DateTime.UtcNow.AddDays(-10), DayPrice = 2 });
    if (status == EquipmentMovementStatus.Completed)
    {
        db.SiteEquipments.Add(new SiteEquipment { Site = f.B, Equipment = f.Equipment, Quantity = 5 });
        db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = f.B, Equipment = f.Equipment,
            Quantity = 5, DateFrom = DateTime.UtcNow.AddDays(-5), DayPrice = 2 });
    }
    db.SiteEquipments.Add(new SiteEquipment { Site = f.C, Equipment = f.Replacement, Quantity = 12 });
    db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = f.C, Equipment = f.Replacement,
        Quantity = 12, DateFrom = DateTime.UtcNow.AddDays(-4), DayPrice = 3 });
    await db.SaveChangesAsync();
    return f;
}
EditMovementItemDto Item(Guid id, int quantity) => new EditMovementItemDto { EquipmentId = id, Quantity = quantity };
async Task Wz(Fixture f, Guid target, Guid equipment, int quantity, Guid? order = null, bool admin = true)
{
    await using var db = NewDb();
    await new EditWzCommandHandler(db, new TestUser(admin)).Handle(new EditWzCommand {
        Id = f.Movement.Id, ToConstructionSiteId = target, OrderId = order, Note = " Corrected ",
        Items = new List<EditMovementItemDto> { Item(equipment, quantity) } }, default);
}
async Task Transfer(Fixture f, Guid source, Guid target, Guid equipment, int quantity, bool admin = true)
{
    await using var db = NewDb();
    await new EditTransferCommandHandler(db, new TestUser(admin)).Handle(new EditTransferCommand {
        Id = f.Movement.Id, FromConstructionSiteId = source, ToConstructionSiteId = target, Note = " Corrected ",
        Items = new List<EditMovementItemDto> { Item(equipment, quantity) } }, default);
}
async Task<int> Stock(Guid site, Guid equipment)
{
    await using var db = NewDb();
    var stock = await db.SiteEquipments.FirstOrDefaultAsync(s => s.SiteId == site && s.EquipmentId == equipment);
    if (stock == null) return 0;
    return stock.Quantity;
}
async Task CheckAllocations(Fixture f)
{
    await using var db = NewDb();
    foreach (var site in new[] { f.A, f.B, f.C })
    {
        foreach (var equipment in new[] { f.Equipment, f.Replacement })
        {
            var allocations = await db.SiteEquipmentAllocations.Where(a => a.SiteId == site.Id &&
                a.EquipmentId == equipment.Id && a.DateTo == null).ToListAsync();
            int quantity = 0;
            foreach (var allocation in allocations) quantity += allocation.Quantity;
            if (quantity != await Stock(site.Id, equipment.Id)) throw new Exception("Allocation mismatch");
        }
    }
    Check(true, "active allocations match current site stocks");
}
bool created = false;
try
{
    await using (var db = NewDb()) { await db.Database.EnsureCreatedAsync(); created = true; }
    var f = await Seed(EquipmentMovementType.WZ, EquipmentMovementStatus.InTransit);
    await Wz(f, f.C.Id, f.Equipment.Id, 7);
    await using (var db = NewDb())
    {
        var equipment = await db.Equipments.FirstAsync(e => e.Id == f.Equipment.Id);
        Check(equipment.QuantityAvailable == 8 && equipment.QuantityInTransit == 7 && await Stock(f.C.Id, equipment.Id) == 0,
            "in-transit WZ changes warehouse and transit, not destination stock");
    }
    await Wz(f, f.C.Id, f.Replacement.Id, 4);
    await using (var db = NewDb())
    {
        var old = await db.Equipments.FirstAsync(e => e.Id == f.Equipment.Id);
        var replacement = await db.Equipments.FirstAsync(e => e.Id == f.Replacement.Id);
        Check(old.QuantityAvailable == 15 && old.QuantityInTransit == 0 && replacement.QuantityAvailable == 6 && replacement.QuantityInTransit == 4,
            "in-transit WZ equipment replacement restores old equipment");
    }
    f = await Seed(EquipmentMovementType.WZ, EquipmentMovementStatus.Completed);
    await Wz(f, f.B.Id, f.Equipment.Id, 3);
    Check(await Stock(f.B.Id, f.Equipment.Id) == 3, "completed WZ reduces destination stock");
    await Wz(f, f.C.Id, f.Equipment.Id, 4);
    Check(await Stock(f.B.Id, f.Equipment.Id) == 0 && await Stock(f.C.Id, f.Equipment.Id) == 4,
        "completed WZ moves stock to corrected destination");
    await Wz(f, f.C.Id, f.Replacement.Id, 2);
    await using (var db = NewDb())
    {
        var old = await db.Equipments.FirstAsync(e => e.Id == f.Equipment.Id);
        var replacement = await db.Equipments.FirstAsync(e => e.Id == f.Replacement.Id);
        Check(old.QuantityAvailable == 15 && replacement.QuantityAvailable == 8 && await Stock(f.C.Id, replacement.Id) == 14,
            "completed WZ swaps equipment and balances warehouse");
    }
    await CheckAllocations(f);
    f = await Seed(EquipmentMovementType.Transfer, EquipmentMovementStatus.InTransit);
    await Transfer(f, f.A.Id, f.C.Id, f.Equipment.Id, 7);
    await using (var db = NewDb())
    {
        var equipment = await db.Equipments.FirstAsync(e => e.Id == f.Equipment.Id);
        Check(equipment.QuantityAvailable == 10 && equipment.QuantityInTransit == 7 && await Stock(f.A.Id, equipment.Id) == 8 &&
            await Stock(f.C.Id, equipment.Id) == 0, "in-transit transfer changes source and transit only");
    }
    await Transfer(f, f.C.Id, f.B.Id, f.Replacement.Id, 4);
    Check(await Stock(f.A.Id, f.Equipment.Id) == 15 && await Stock(f.C.Id, f.Replacement.Id) == 8,
        "in-transit transfer source and equipment replacement");
    await CheckAllocations(f);
    f = await Seed(EquipmentMovementType.Transfer, EquipmentMovementStatus.Completed);
    await Transfer(f, f.A.Id, f.B.Id, f.Equipment.Id, 3);
    Check(await Stock(f.A.Id, f.Equipment.Id) == 12 && await Stock(f.B.Id, f.Equipment.Id) == 3,
        "completed transfer adjusts both sites by quantity difference");
    await Transfer(f, f.C.Id, f.A.Id, f.Replacement.Id, 4);
    Check(await Stock(f.A.Id, f.Equipment.Id) == 15 && await Stock(f.B.Id, f.Equipment.Id) == 0 &&
        await Stock(f.C.Id, f.Replacement.Id) == 8 && await Stock(f.A.Id, f.Replacement.Id) == 4,
        "completed transfer corrects both sites and equipment");
    await CheckAllocations(f);
    f = await Seed(EquipmentMovementType.Transfer, EquipmentMovementStatus.Completed);
    // B ma dodatkowy sprzęt z innych dokumentów, więc odwrócenie kierunku jest możliwe.
    await using (var db = NewDb())
    {
        var stock = await db.SiteEquipments.FirstAsync(s => s.SiteId == f.B.Id && s.EquipmentId == f.Equipment.Id);
        stock.Quantity = 12;
        var allocation = await db.SiteEquipmentAllocations.FirstAsync(a => a.SiteId == f.B.Id && a.EquipmentId == f.Equipment.Id);
        allocation.Quantity = 12;
        await db.SaveChangesAsync();
    }
    await Transfer(f, f.B.Id, f.A.Id, f.Equipment.Id, 4);
    Check(await Stock(f.A.Id, f.Equipment.Id) == 19 && await Stock(f.B.Id, f.Equipment.Id) == 3,
        "reversing completed transfer direction uses net site differences");
    await CheckAllocations(f);
    bool rejected = false;
    try { await Transfer(f, f.B.Id, f.A.Id, f.Equipment.Id, 50); } catch (InvalidOperationException) { rejected = true; }
    await using (var db = NewDb())
    {
        var document = await db.EquipmentMovements.Include(m => m.Items).FirstAsync(m => m.Id == f.Movement.Id);
        Check(rejected && document.Items[0].Quantity == 4 && await Stock(f.A.Id, f.Equipment.Id) == 19 &&
            await Stock(f.B.Id, f.Equipment.Id) == 3, "impossible correction rolls back document and stocks");
    }
    rejected = false;
    try { await Transfer(f, f.B.Id, f.A.Id, f.Equipment.Id, 4, false); } catch (UnauthorizedAccessException) { rejected = true; }
    Check(rejected, "non-admin cannot edit transfer");
    rejected = false;
    try { await Wz(f, f.A.Id, f.Equipment.Id, 4); } catch (KeyNotFoundException) { rejected = true; }
    Check(rejected, "WZ endpoint does not edit a transfer");
    f = await Seed(EquipmentMovementType.WZ, EquipmentMovementStatus.Completed);
    await using (var db = NewDb())
    {
        var equipment = await db.Equipments.FirstAsync(e => e.Id == f.Equipment.Id);
        equipment.QuantityAvailable = 0;
        await db.SaveChangesAsync();
    }
    await Wz(f, f.B.Id, f.Equipment.Id, 5);
    await using (var db = NewDb())
    {
        Check((await db.Equipments.FirstAsync(e => e.Id == f.Equipment.Id)).QuantityAvailable == 0 &&
            await db.SiteEquipmentAllocations.CountAsync(a => a.SiteId == f.B.Id) == 1,
            "unchanged quantities at zero warehouse stock do not create allocations");
    }
    rejected = false;
    try { await Wz(f, f.B.Id, f.Equipment.Id, 5, admin: false); } catch (UnauthorizedAccessException) { rejected = true; }
    Check(rejected, "non-admin cannot edit WZ");
    f = await Seed(EquipmentMovementType.WZ, EquipmentMovementStatus.InTransit);
    var order = new Order { SiteId = f.B.Id, OrderNumber = "TEST-" + Guid.NewGuid(), Status = OrderStatus.Completed,
        Items = new List<OrderItem> { new OrderItem { EquipmentId = f.Equipment.Id, Quantity = 5 } } };
    await using (var db = NewDb())
    {
        db.Orders.Add(order);
        var movement = await db.EquipmentMovements.FirstAsync(m => m.Id == f.Movement.Id);
        movement.OrderId = order.Id;
        await db.SaveChangesAsync();
    }
    await Wz(f, f.B.Id, f.Equipment.Id, 3, order.Id);
    await using (var db = NewDb())
    {
        Check((await db.Orders.FirstAsync(o => o.Id == order.Id)).Status == OrderStatus.PartiallySent,
            "reducing WZ recalculates order status");
    }
    rejected = false;
    try { await Wz(f, f.C.Id, f.Equipment.Id, 3, order.Id); } catch (InvalidOperationException) { rejected = true; }
    Check(rejected, "linked order prevents incompatible site correction");
    await Wz(f, f.C.Id, f.Equipment.Id, 3);
    await using (var db = NewDb())
    {
        var movement = await db.EquipmentMovements.FirstAsync(m => m.Id == f.Movement.Id);
        Check(movement.OrderId == null && movement.ToConstructionSiteId == f.C.Id &&
            (await db.Orders.FirstAsync(o => o.Id == order.Id)).Status == OrderStatus.Pending,
            "unlinking corrected WZ releases order quantities");
    }
    var invalidWz = new EditWzCommand { Id = Guid.NewGuid(), ToConstructionSiteId = f.B.Id,
        Items = new List<EditMovementItemDto> { Item(f.Equipment.Id, 1), Item(f.Equipment.Id, 1) } };
    var invalidTransfer = new EditTransferCommand { Id = Guid.NewGuid(), FromConstructionSiteId = f.A.Id,
        ToConstructionSiteId = f.A.Id, Items = new List<EditMovementItemDto> { Item(f.Equipment.Id, 0) } };
    Check(!new EditWzCommandValidator().Validate(invalidWz).IsValid &&
        !new EditTransferCommandValidator().Validate(invalidTransfer).IsValid,
        "validators reject duplicate equipment, nonpositive quantities and identical sites");
    Console.WriteLine("All " + checks + " checks passed.");
}
finally
{
    if (created)
    {
        await using var db = NewDb();
        if (new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog != database)
            throw new Exception("Unexpected database");
        await db.Database.EnsureDeletedAsync();
        Console.WriteLine("Temporary test database removed.");
    }
}

class Fixture
{
    public ApplicationUser User = new ApplicationUser { UserName = "test-" + Guid.NewGuid() };
    public Site A = new Site { Name = "A-" + Guid.NewGuid(), Status = SiteStatus.InProgress };
    public Site B = new Site { Name = "B-" + Guid.NewGuid(), Status = SiteStatus.InProgress };
    public Site C = new Site { Name = "C-" + Guid.NewGuid(), Status = SiteStatus.InProgress };
    public Equipment Equipment = new Equipment { Name = "Equipment-" + Guid.NewGuid(), Unit = "szt", Category = new Category { Name = "Test-" + Guid.NewGuid() }, QuantityAvailable = 10, DayPrice = 2 };
    public Equipment Replacement = new Equipment { Name = "Replacement-" + Guid.NewGuid(), Unit = "szt", Category = new Category { Name = "Test-" + Guid.NewGuid() }, QuantityAvailable = 10, DayPrice = 3 };
    public EquipmentMovement Movement = new EquipmentMovement { Number = "TEST/" + Guid.NewGuid() };
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


