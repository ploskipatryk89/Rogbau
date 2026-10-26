using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rogbau.Api.Features.Purchases.Commands;
using Rogbau.Api.Features.Purchases.Commands.EditPurchase;
using Rogbau.Api.Features.Purchases.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

var source = new ConfigurationBuilder().SetBasePath(Path.GetFullPath(args[0]))
    .AddJsonFile("appsettings.json").AddJsonFile("appsettings.Development.json", optional: true).Build();
var connection = new SqlConnectionStringBuilder(source.GetConnectionString("DefaultConnection"));
var database = "Rogbau_PurchaseEditChecks_" + Guid.NewGuid().ToString("N");
connection.InitialCatalog = database;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
ApplicationDbContext NewDb() => new(options);
void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAIL: " + message);
    Console.WriteLine("PASS: " + message);
}
CreatePurchaseItemDto Item(Guid id, int quantity, decimal price) => new() { EquipmentId = id, Quantity = quantity, PurchasePrice = price };
var date = new DateOnly(2026, 10, 10);
var supplier = new Supplier { Name = "Purchase test" };
var category = new Category { Name = "Purchase test" };
var a = new Equipment { Name = "A", Unit = "szt", Category = category };
var b = new Equipment { Name = "B", Unit = "szt", Category = category };
var created = false;
try
{
    await using (var db = NewDb())
    {
        await db.Database.EnsureCreatedAsync();
        created = true;
        db.AddRange(supplier, a, b);
        await db.SaveChangesAsync();
    }
    async Task<Guid> Create(int quantity, decimal price)
    {
        await using var db = NewDb();
        return await new CreatePurchaseCommandHandler(db).Handle(new CreatePurchaseCommand
        { SupplierId = supplier.Id, PurchaseDate = date, Items = [Item(a.Id, quantity, price)] }, default);
    }
    var purchaseId = await Create(10, 10);
    await Create(10, 20);
    async Task Edit(params CreatePurchaseItemDto[] items)
    {
        await using var db = NewDb();
        await new EditPurchaseCommandHandler(db).Handle(new EditPurchaseCommand
        { Id = purchaseId, SupplierId = supplier.Id, PurchaseDate = date.AddDays(1), Items = items.ToList() }, default);
    }
    await Edit(Item(a.Id, 15, 12));
    await using (var db = NewDb())
    {
        var equipment = await db.Equipments.FindAsync(a.Id);
        Check(equipment!.QuantityAvailable == 25 && equipment.PurchasedQuantity == 25 && equipment.PurchasePrice == 15.2m,
            "increase adjusts stock and weighted purchase price");
        var purchase = await db.Purchases.Include(p => p.Items).SingleAsync(p => p.Id == purchaseId);
        Check(purchase.TotalPurchasePrice == 180 && purchase.Items.Count == 1 && purchase.PurchaseDate == date.AddDays(1),
            "purchase items, total and date persist");
    }
    await Edit(Item(b.Id, 5, 30));
    await using (var db = NewDb())
    {
        var first = await db.Equipments.FindAsync(a.Id);
        var second = await db.Equipments.FindAsync(b.Id);
        Check(first!.QuantityAvailable == 10 && first.PurchasedQuantity == 10 && first.PurchasePrice == 20 && second!.QuantityAvailable == 5,
            "equipment replacement reverses old stock and adds new stock");
        second!.QuantityAvailable = 1; // Four units have left the warehouse.
        await db.SaveChangesAsync();
    }
    var rejected = false;
    try { await Edit(Item(a.Id, 2, 10)); }
    catch (InvalidOperationException) { rejected = true; }
    Check(rejected, "reduction below available stock is rejected");
    await using (var db = NewDb())
    {
        var first = await db.Equipments.FindAsync(a.Id);
        var second = await db.Equipments.FindAsync(b.Id);
        var purchase = await db.Purchases.Include(p => p.Items).SingleAsync(p => p.Id == purchaseId);
        Check(first!.QuantityAvailable == 10 && second!.QuantityAvailable == 1 && purchase.Items.Single().EquipmentId == b.Id,
            "rejected edit rolls back all equipment and purchase changes");
    }
    await Edit(Item(b.Id, 4, 35));
    await using (var db = NewDb())
    {
        var equipment = await db.Equipments.FindAsync(b.Id);
        Check(equipment!.QuantityAvailable == 0 && equipment.PurchasedQuantity == 4 && equipment.PurchasePrice == 35,
            "allowed reduction consumes only available warehouse stock");
    }
    var validator = new EditPurchaseCommandValidator();
    Check(!validator.Validate(new EditPurchaseCommand { Id = purchaseId, SupplierId = supplier.Id, PurchaseDate = date,
        Items = [Item(a.Id, 1, 10), Item(a.Id, 1, 10)] }).IsValid, "duplicate equipment is rejected");
    Check(!validator.Validate(new EditPurchaseCommand { Id = purchaseId, SupplierId = supplier.Id, PurchaseDate = date,
        Items = [Item(a.Id, 0, 1.234m)] }).IsValid, "invalid quantity and price precision are rejected");
}
finally
{
    if (created)
    {
        await using var db = NewDb();
        if (new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog != database)
            throw new Exception("Unexpected cleanup database.");
        await db.Database.EnsureDeletedAsync();
        Console.WriteLine("Temporary test database removed.");
    }
}
