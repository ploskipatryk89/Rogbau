using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Rogbau.Api.Common.Exceptions;
using Rogbau.Api.Features.Equipments.Commands.DeleteEquipment;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

var configuration = new ConfigurationBuilder().SetBasePath(Path.GetFullPath(args[0]))
    .AddJsonFile("appsettings.json").Build();
var connection = new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
var databaseName = "Rogbau_EquipmentDeleteChecks_" + Guid.NewGuid().ToString("N");
connection.InitialCatalog = databaseName;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
var created = false;
void Check(bool result, string message)
{
    if (!result) throw new Exception("FAIL: " + message);
    Console.WriteLine("PASS: " + message);
}
async Task Delete(Guid id)
{
    await using var context = new ApplicationDbContext(options);
    var handler = new DeleteEquipmentCommandHandler(context, NullLogger<DeleteEquipmentCommandHandler>.Instance);
    await handler.Handle(new DeleteEquipmentCommand(id), default);
}
try
{
    await using (var context = new ApplicationDbContext(options))
    {
        await context.Database.EnsureCreatedAsync();
        created = true;
    }
    foreach (var history in new[] { "none", "purchase", "order", "movement", "site", "allocation" })
    {
        var equipment = new Equipment { Name = history, Unit = "szt", Category = new Category { Name = history } };
        await using (var context = new ApplicationDbContext(options))
        {
            context.Equipments.Add(equipment);
            if (history == "purchase")
                context.PurchaseItems.Add(new PurchaseItem { Equipment = equipment, Quantity = 1,
                    Purchase = new Purchase { PurchaseNumber = "TEST-1", Supplier = new Supplier { Name = "Test" } } });
            if (history == "order")
                context.Orders.Add(new Order { Site = new Site { Name = history },
                    Items = new List<OrderItem> { new OrderItem { Equipment = equipment, Quantity = 1 } } });
            if (history == "movement")
                context.EquipmentMovementItems.Add(new EquipmentMovementItem { Equipment = equipment, Quantity = 1,
                    EquipmentMovement = new EquipmentMovement { AddedBy = new ApplicationUser { UserName = "Test" } } });
            if (history == "site")
                context.SiteEquipments.Add(new SiteEquipment { Equipment = equipment, Site = new Site { Name = history }, Quantity = 0 });
            if (history == "allocation")
                context.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Equipment = equipment,
                    Site = new Site { Name = history }, Quantity = 1, DateFrom = DateTime.UtcNow.AddDays(-2), DateTo = DateTime.UtcNow.AddDays(-1) });
            await context.SaveChangesAsync();
        }
        if (history == "none")
        {
            await Delete(equipment.Id);
            await using var context = new ApplicationDbContext(options);
            Check(!await context.Equipments.AnyAsync(x => x.Id == equipment.Id), "unused equipment is deleted");
            bool notFound = false;
            try { await Delete(equipment.Id); }
            catch (KeyNotFoundException) { notFound = true; }
            Check(notFound, "missing equipment returns not found");
        }
        else
        {
            bool rejected = false;
            try { await Delete(equipment.Id); }
            catch (ConflictException error) { rejected = error.Message.Contains("historię dokumentów"); }
            Check(rejected, history + " returns readable conflict");
            await using var context = new ApplicationDbContext(options);
            Check(await context.Equipments.AnyAsync(x => x.Id == equipment.Id), history + " preserves equipment");
            if (history == "allocation")
                Check(await context.SiteEquipmentAllocations.AnyAsync(x => x.EquipmentId == equipment.Id), "closed allocation history is preserved");
        }
    }
}
finally
{
    if (created)
    {
        await using var context = new ApplicationDbContext(options);
        if (new SqlConnectionStringBuilder(context.Database.GetConnectionString()).InitialCatalog != databaseName)
            throw new Exception("Unexpected cleanup database");
        await context.Database.EnsureDeletedAsync();
        Console.WriteLine("Temporary test database removed.");
    }
}

