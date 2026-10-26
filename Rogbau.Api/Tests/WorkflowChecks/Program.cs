using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rogbau.Api.Common.Extensions;
using Rogbau.Api.Features.EquipmentTransfer.Commands.ConfirmEquipmentMovement;
using Rogbau.Api.Features.EquipmentTransfer.Commands.CreateTransfer;
using Rogbau.Api.Features.EquipmentTransfer.Commands.CreateWz;
using Rogbau.Api.Features.EquipmentTransfer.Commands.CreatePz;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Features.EquipmentTransfer.Queries.GetAllTransfers;
using Rogbau.Api.Features.EquipmentTransfer.Queries.GetTransferById;
using Rogbau.Api.Features.Orders.Commands.CreateOrder;
using Rogbau.Api.Features.Orders.Dtos;
using Rogbau.Api.Features.Orders.Queries;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Rogbau.Api.Features.Dashboard.Dtos;
using Rogbau.Api.Features.Dashboard.Queries.GetDashboard;
using Rogbau.Api.Features.Categories.Commands.CreateCategory;
using Rogbau.Api.Features.Categories.Commands.EditCategory;
using Rogbau.Api.Features.Categories.Commands.DeleteCategory;
using Rogbau.Api.Features.Categories.Commands.CreateSubCategory;
using Rogbau.Api.Features.Categories.Commands.EditSubCategory;
using Rogbau.Api.Features.Categories.Commands.DeleteSubCategory;

// An isolated database is created for this run; no project data is changed.
var apiPath = Path.GetFullPath(args.FirstOrDefault() ?? "../../Rogbau.Api");
var source = new ConfigurationBuilder().SetBasePath(apiPath).AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true).Build();
var connection = new SqlConnectionStringBuilder(source.GetConnectionString("DefaultConnection"));
var testDatabase = "Rogbau_WorkflowChecks_" + Guid.NewGuid().ToString("N");
connection.InitialCatalog = testDatabase;
var config = new ConfigurationBuilder().AddConfiguration(source).AddInMemoryCollection(
    new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = connection.ConnectionString }).Build();
var currentUser = new TestUser();
var services = new ServiceCollection();
services.AddLogging(x => x.SetMinimumLevel(LogLevel.Error));
services.AddDatabaseAndIdentity(config);
services.AddJwtAuthentication(config);
services.AddApplicationServices();
services.AddSignalR();
services.AddAuthorization();
services.AddSingleton<ICurrentUserService>(currentUser);
services.AddSingleton<IHubContext<NotificationHub>, TestHub>();
await using var provider = services.BuildServiceProvider();
var created = false;
var passed = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new Exception("FAIL: " + label);
    Console.WriteLine("PASS: " + label); passed++;
}
async Task<T> Send<T>(IRequest<T> request)
{
    using var scope = provider.CreateScope();
    return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request);
}
async Task Confirm(Guid id, List<ConfirmReturnItem>? items = null)
{
    using var scope = provider.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new ConfirmEquipmentMovementCommand { Id = id, Items = items });
}
async Task Denied(Func<Task> action, string label)
{
    try { await action(); }
    catch (Exception e) when (e is Rogbau.Api.Common.Exceptions.ValidationException or FluentValidation.ValidationException or UnauthorizedAccessException or KeyNotFoundException or ArgumentException or InvalidOperationException or Rogbau.Api.Common.Exceptions.ConflictException)
    { Check(true, label); return; }
    throw new Exception("FAIL: " + label);
}

async Task InvalidField(Func<Task> action, string field)
{
    try { await action(); }
    catch (FluentValidation.ValidationException e)
    {
        Check(e.Errors.Any(error => error.PropertyName == field), "validation pipeline reports field " + field);
        return;
    }
    catch (Rogbau.Api.Common.Exceptions.ValidationException e)
    {
        Check(e.Errors.ContainsKey(field), "validation pipeline reports field " + field);
        return;
    }
    throw new Exception("FAIL: expected validation error for " + field);
}

try
{
    using var setup = provider.CreateScope();
    var db = setup.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    created = true;
    await db.Database.MigrateAsync();
    Check(!db.Database.HasPendingModelChanges(), "migrations match current application model");
    var roles = setup.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var users = setup.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    foreach (var role in new[] { "Admin", "Magazynier", "Kierownik Budowy" })
        Check((await roles.CreateAsync(new IdentityRole(role))).Succeeded, "create test role " + role);
    async Task<ApplicationUser> NewUser(string name, string role)
    {
        var user = new ApplicationUser { UserName = name, Email = name + "@test.invalid" };
        Check((await users.CreateAsync(user)).Succeeded, "create test user " + name);
        Check((await users.AddToRoleAsync(user, role)).Succeeded, "assign test role " + name);
        return user;
    }
    var admin = await NewUser("admin", "Admin");
    var warehouse = await NewUser("warehouse", "Magazynier");
    var managerA = await NewUser("managerA", "Kierownik Budowy");
    var managerB = await NewUser("managerB", "Kierownik Budowy");
    // Exercise the controller's real authorization attributes with generated JWTs.
    var wzEndpoint = typeof(Rogbau.Api.Features.EquipmentTransfer.EquipmentTransferController)
        .GetMethods().Single(m => m.GetParameters().Any(p => p.ParameterType == typeof(CreateWzCommand)));
    var wzRoles = wzEndpoint.GetCustomAttributes(typeof(AuthorizeAttribute), true)
        .Cast<AuthorizeAttribute>().Single(a => a.Roles != null).Roles!.Split(',').Select(r => r.Trim()).ToArray();
    var wzPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(wzRoles).Build();
    foreach (var testCase in new[] { (User: admin, Allowed: true), (User: warehouse, Allowed: true), (User: managerA, Allowed: false) })
    {
        var token = await setup.ServiceProvider.GetRequiredService<IJwtTokenService>().GenerateTokenAsync(testCase.User);
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidIssuer = config["Jwt:Issuer"], ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true
        }, out _);
        var result = await setup.ServiceProvider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(principal, null, wzPolicy);
        Check(result.Succeeded == testCase.Allowed, "WZ endpoint JWT authorization for " + testCase.User.UserName);
    }
    var siteA = new Site { Name = "Site A" };
    var today = DateOnly.FromDateTime(DateTime.Today);
    Check(new Site { Status = SiteStatus.InProgress, StartDate = today.AddDays(1) }.GetCurrentStatus(today) == SiteStatus.Planned,
        "future start date corrects previously stored in-progress status");
    Check(new Site { Status = SiteStatus.Planned, StartDate = today }.GetCurrentStatus(today) == SiteStatus.InProgress,
        "scheduled site becomes active on its start date");
    Check(new Site { Status = SiteStatus.Completed, StartDate = today.AddDays(-1) }.GetCurrentStatus(today) == SiteStatus.Completed,
        "completed site remains completed");
    foreach (var endpoint in new[] {
        (typeof(Rogbau.Api.Features.Users.UserController), "Update"),
        (typeof(Rogbau.Api.Features.Equipments.EquipmentController), "Update"),
        (typeof(Rogbau.Api.Features.Sites.SitesController), "SetInitialStock"),
        (typeof(Rogbau.Api.Features.Sites.SitesController), "StartSite"),
        (typeof(Rogbau.Api.Features.Sites.SitesController), "FinishSite"),
        (typeof(Rogbau.Api.Features.Suppliers.SuppliersController), "Create") })
    {
        var metadata = endpoint.Item1.GetMethod(endpoint.Item2)!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
            .RequireRole(metadata.Single(a => a.Roles != null).Roles!.Split(',').Select(r => r.Trim())).Build();
        var authorization = setup.ServiceProvider.GetRequiredService<IAuthorizationService>();
        foreach (var role in new[] { "Admin", "Magazynier", "Kierownik Budowy" })
        {
            var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)], "Test"));
            var allowed = role == "Admin" || (role == "Kierownik Budowy" && endpoint.Item2 is "StartSite" or "SetInitialStock");
            Check((await authorization.AuthorizeAsync(principal, null, policy)).Succeeded == allowed,
                endpoint.Item1.Name + "." + endpoint.Item2 + " authorization for " + role);
        }
        Check(!(await authorization.AuthorizeAsync(new System.Security.Claims.ClaimsPrincipal(), null, policy)).Succeeded,
            endpoint.Item1.Name + "." + endpoint.Item2 + " rejects anonymous requests");
    }
    var siteB = new Site { Name = "Site B" };
    var siteC = new Site { Name = "Site C" };
    var category = new Category { Name = "Test" };
    var equipment = new Equipment { Name = "Head", Unit = "szt", WeightKg = 2.5m,
        QuantityAvailable = 12, DayPrice = 1, Category = category };
    db.AddRange(siteA, siteB, siteC, equipment);
    db.SiteUsers.AddRange(new SiteUser { Site = siteA, UserId = managerA.Id },
        new SiteUser { Site = siteB, UserId = managerB.Id });
    await db.SaveChangesAsync();

    currentUser.Set(managerA.Id, "Kierownik Budowy");
    var orderId = await Send(new CreateOrderCommand { SiteId = siteA.Id, Note = "test note",
        Items = [new CreateOrderItemDto { EquipmentId = equipment.Id, Quantity = 20 }] });
    Check((await Send(new GetOrdersQuery(orderId))).Single().Items.Single().Quantity == 20,
        "order accepts quantity above current warehouse stock");
    Check(await db.Recipients.CountAsync(x => x.Notification.ActionUrl == "/orders/" + orderId) == 2,
        "order notification is saved for admin and warehouse");
    await Denied(async () => { await Send(new CreateOrderCommand { SiteId = siteB.Id,
        Items = [new CreateOrderItemDto { EquipmentId = equipment.Id, Quantity = 1 }] }); }, "cannot order for another site");

    currentUser.Set(warehouse.Id, "Magazynier");
    CreateWzCommand Wz(int quantity, Guid? target = null) => new() { OrderId = orderId,
        ToConstructionSiteId = target ?? siteA.Id,
        Items = [new CreateWzItemDto { EquipmentId = equipment.Id, Quantity = quantity }] };
    var documentCount = await db.EquipmentMovements.CountAsync();
    await InvalidField(async () => { await Send(new CreateWzCommand { ToConstructionSiteId = siteA.Id,
        Note = new string('a', 1001), Items = [new CreateWzItemDto { EquipmentId = equipment.Id, Quantity = 1 }] }); }, "Note");
    await InvalidField(async () => { await Send(new CreatePzCommand { FromConstructionSiteId = siteA.Id,
        Note = new string('a', 1001), Items = [new CreatePzItemDto { EquipmentId = equipment.Id, Quantity = 1 }] }); }, "Note");
    await InvalidField(async () => { await Send(new CreateOrderCommand { SiteId = siteA.Id,
        Note = new string('a', 1001), Items = [new CreateOrderItemDto { EquipmentId = equipment.Id, Quantity = 1 }] }); }, "Note");
    await InvalidField(async () => { await Send(new CreateWzCommand { ToConstructionSiteId = siteA.Id, Items = null! }); }, "Items");
    Check(await db.EquipmentMovements.CountAsync() == documentCount, "invalid documents do not reach persistence");
    var wz1 = await Send(Wz(12));
    currentUser.Set(admin.Id, "Admin");
    Check(!(await Send(new GetTransferByIdQuery(wz1))).CanConfirm, "admin has no receipt confirmation button");
    await Denied(() => Confirm(wz1), "admin cannot confirm receipt through handler");
    var confirmEndpoint = typeof(Rogbau.Api.Features.EquipmentTransfer.EquipmentTransferController).GetMethod("Confirm")!;
    var confirmRoles = confirmEndpoint.GetCustomAttributes(typeof(AuthorizeAttribute), true)
        .Cast<AuthorizeAttribute>().Single(a => a.Roles != null).Roles!;
    Check(confirmRoles == "Admin,Magazynier,Kierownik Budowy", "receipt endpoint includes warehouse for PZ confirmation");
    currentUser.Set(warehouse.Id, "Magazynier");
    var partial = (await Send(new GetOrdersQuery(orderId))).Single();
    Check(partial.Status == OrderStatus.PartiallySent && partial.Items.Single().SentQuantity == 12,
        "first WZ partially fulfills order");
    Check(partial.Transfers.Single().Id == wz1, "WZ is linked to order");
    await Denied(async () => { await Send(Wz(1)); }, "cannot send unavailable warehouse stock");
    await Denied(async () => { await Send(Wz(1, siteB.Id)); }, "cannot issue order WZ to another site");

    currentUser.Set(managerB.Id, "Kierownik Budowy");
    Check((await Send(new GetOrdersQuery(orderId))).Count == 0, "another manager cannot view order");
    Check((await Send(new GetAllTransfersQuery())).Count == 0, "another manager cannot list unrelated WZ");
    await Denied(async () => { await Send(new GetTransferByIdQuery(wz1)); }, "another manager cannot open WZ by id");
    await Denied(() => Confirm(wz1), "another manager cannot confirm WZ");

    currentUser.Set(managerA.Id, "Kierownik Budowy");
    var details = await Send(new GetTransferByIdQuery(wz1));
    Check(details.CanConfirm && details.TotalWeight == 30m, "target manager can confirm WZ; weight matches sent quantity");
    await Confirm(wz1);
    await Denied(() => Confirm(wz1), "WZ cannot be confirmed twice");
    db.ChangeTracker.Clear();
    Check((await db.SiteEquipments.SingleAsync(x => x.SiteId == siteA.Id)).Quantity == 12,
        "receipt adds equipment to target site");

    var stock = await db.Equipments.SingleAsync();
    stock.QuantityAvailable = 10;
    await db.SaveChangesAsync();
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await Send(Wz(9)); }, "cannot exceed remaining order quantity");
    var wz2 = await Send(Wz(8));
    var completed = (await Send(new GetOrdersQuery(orderId))).Single();
    Check(completed.Status == OrderStatus.Completed && completed.Items.Single().SentQuantity == 20 &&
        completed.Transfers.Count == 2, "second WZ completes order and retains both documents");
    await Denied(async () => { await Send(Wz(1)); }, "completed order cannot be oversent");

    currentUser.Set(managerA.Id, "Kierownik Budowy");
    var transferId = await Send(new CreateTransferCommand { FromConstructionSiteId = siteA.Id,
        ToConstructionSiteId = siteB.Id,
        Items = [new CreateTransferItemDto { EquipmentId = equipment.Id, Quantity = 6 }] });
    Check(!(await Send(new GetTransferByIdQuery(transferId))).CanConfirm, "sending manager cannot confirm transfer");
    currentUser.Set(managerB.Id, "Kierownik Budowy");
    Check((await Send(new GetAllTransfersQuery())).Single().Id == transferId, "target manager sees incoming transfer only");
    Check((await Send(new GetTransferByIdQuery(transferId))).CanConfirm, "target manager can confirm transfer");
    await Denied(async () => { await Send(new CreateTransferCommand { FromConstructionSiteId = siteA.Id,
        ToConstructionSiteId = siteB.Id, Items = [new CreateTransferItemDto { EquipmentId = equipment.Id, Quantity = 1 }] }); },
        "manager cannot send equipment from another site");
    await Confirm(transferId);

    // A manager assigned to several sites sees movements of all of them.
    db.ChangeTracker.Clear();
    db.SiteUsers.Add(new SiteUser { SiteId = siteB.Id, UserId = managerA.Id });
    db.EquipmentMovements.Add(new EquipmentMovement { Number = "TEST/PZ", Type = EquipmentMovementType.PZ,
        FromConstructionSiteId = siteB.Id, Status = EquipmentMovementStatus.Completed, AddedByUserId = admin.Id });
    db.EquipmentMovements.Add(new EquipmentMovement { Number = "TEST/OTHER", Type = EquipmentMovementType.WZ,
        ToConstructionSiteId = siteC.Id, Status = EquipmentMovementStatus.InTransit, AddedByUserId = admin.Id });
    await db.SaveChangesAsync();
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    Check((await Send(new GetAllTransfersQuery())).Count == 4, "manager sees WZ, PZ and transfers for multiple assigned sites");
    currentUser.Set(admin.Id, "Admin");
    Check((await Send(new GetAllTransfersQuery())).Count == 5, "admin sees all movements");
    currentUser.Set(warehouse.Id, "Magazynier");
    Check((await Send(new GetAllTransfersQuery())).Count == 5, "warehouse sees all movements");
    Check(TestHub.Deliveries.Any(x => x.Ids.Contains(admin.Id) && x.Ids.Contains(warehouse.Id)),
        "new order is dispatched through SignalR to admin and warehouse");
    Check(TestHub.Deliveries.Any(x => x.Ids.Contains(managerA.Id)), "WZ notification is dispatched to target manager");
    foreach (var scenario in new[] {
        (Stock: 15, Allocated: 0, Returned: 10, Scrapped: 2, Role: "Admin"),
        (Stock: 5, Allocated: 3, Returned: 4, Scrapped: 0, Role: "Magazynier"),
        (Stock: 10, Allocated: 10, Returned: 4, Scrapped: 1, Role: "Admin") })
    {
        db.ChangeTracker.Clear();
        var returnSite = new Site { Name = "PZ test " + scenario.Stock };
        var returnEquipment = new Equipment { Name = "PZ equipment " + scenario.Stock, Unit = "szt", CategoryId = category.Id };
        db.SiteEquipments.Add(new SiteEquipment { Site = returnSite, Equipment = returnEquipment, Quantity = scenario.Stock });
        if (scenario.Allocated > 0)
            db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = returnSite, Equipment = returnEquipment,
                Quantity = scenario.Allocated, DateFrom = DateTime.UtcNow.AddDays(-2), DayPrice = 1 });
        await db.SaveChangesAsync();
        currentUser.Set(managerA.Id, "Kierownik Budowy");
        CreatePzCommand Pz(int quantity, int scrap) => new() { FromConstructionSiteId = returnSite.Id, Note = " Zwrot z budowy ",
            Items = [new CreatePzItemDto { EquipmentId = returnEquipment.Id, Quantity = quantity, QuantityScrapped = scrap }] };
        await Denied(async () => { await Send(Pz(1, 0)); }, "unassigned manager cannot create PZ");
        db.SiteUsers.Add(new SiteUser { SiteId = returnSite.Id, UserId = managerA.Id });
        await db.SaveChangesAsync();
        currentUser.Set(admin.Id, "Admin");
        await Denied(async () => { await Send(Pz(1, 0)); }, "admin receives returns instead of creating PZ");
        currentUser.Set(warehouse.Id, "Magazynier");
        await Denied(async () => { await Send(Pz(1, 0)); }, "warehouse receives returns instead of creating PZ");
        currentUser.Set(managerA.Id, "Kierownik Budowy");
        await Denied(async () => { await Send(Pz(scenario.Stock + 1, 0)); }, "PZ cannot exceed site stock");
        await Denied(async () => { await Send(Pz(1, 2)); }, "PZ scrap cannot exceed received quantity");
        var pzId = await Send(Pz(scenario.Returned, scenario.Scrapped));
        Check((await Send(new GetTransferByIdQuery(pzId))).Note == "Zwrot z budowy", "PZ preserves trimmed notes");
        db.ChangeTracker.Clear();
        var received = await db.Equipments.SingleAsync(x => x.Id == returnEquipment.Id);
        Check(received.QuantityAvailable == 0 && received.QuantityScrapped == 0 && received.QuantityInTransit == scenario.Returned,
            "PZ reserves return in transit without adding warehouse stock or scrap");
        Check((await db.SiteEquipments.SingleAsync(x => x.SiteId == returnSite.Id)).Quantity == scenario.Stock - scenario.Returned,
            "PZ subtracts actual received quantity from source site");
        var active = await db.SiteEquipmentAllocations.Where(x => x.SiteId == returnSite.Id && x.DateTo == null)
            .SumAsync(x => (int?)x.Quantity) ?? 0;
        Check(active == Math.Max(0, scenario.Allocated - scenario.Returned), "PZ closes only existing allocation history");
        currentUser.Set(scenario.Role == "Admin" ? admin.Id : warehouse.Id, scenario.Role);
        Check((await Send(new GetTransferByIdQuery(pzId))).Status == EquipmentMovementStatus.InTransit &&
            (await Send(new GetTransferByIdQuery(pzId))).CanConfirm, "warehouse can confirm pending PZ");
        currentUser.Set(managerA.Id, "Kierownik Budowy");
        await Denied(() => Confirm(pzId), "manager cannot confirm PZ");
        currentUser.Set(scenario.Role == "Admin" ? admin.Id : warehouse.Id, scenario.Role);
        await Confirm(pzId);
        db.ChangeTracker.Clear();
        received = await db.Equipments.SingleAsync(x => x.Id == returnEquipment.Id);
        Check(received.QuantityAvailable == scenario.Returned - scenario.Scrapped &&
            received.QuantityScrapped == scenario.Scrapped && received.QuantityInTransit == 0,
            "confirmed PZ adds usable stock and scrap exactly once");
        await Denied(() => Confirm(pzId), "PZ cannot be confirmed twice");
        currentUser.Set(managerA.Id, "Kierownik Budowy");
        var managerPz = await Send(Pz(1, 0));
        Check(!(await Send(new GetTransferByIdQuery(managerPz))).CanConfirm,
            "assigned manager can create return but cannot accept it");
        currentUser.Set(managerB.Id, "Kierownik Budowy");
        await Denied(async () => { await Send(Pz(1, 0)); }, "another manager cannot return assigned site's equipment");
        currentUser.Set(warehouse.Id, "Magazynier");
        Check((await Send(new GetTransferByIdQuery(managerPz))).CanConfirm,
            "warehouse can receive manager's PZ");
        Check(await db.Recipients.AsNoTracking().AnyAsync(r => r.UserId == warehouse.Id &&
            r.Notification.ActionUrl == "/transfers/" + managerPz), "return notifies warehouse");
        await Confirm(managerPz);
    }
    foreach (var actual in new[] { 0, 2, 6 })
    {
        db.ChangeTracker.Clear();
        var correctionSite = new Site { Name = "Receipt correction " + actual };
        var correctionEquipment = new Equipment { Name = "Receipt correction " + actual, Unit = "szt",
            CategoryId = category.Id, DayPrice = 7 };
        db.SiteEquipments.Add(new SiteEquipment { Site = correctionSite, Equipment = correctionEquipment, Quantity = 10 });
        db.SiteUsers.Add(new SiteUser { Site = correctionSite, UserId = managerA.Id });
        db.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation { Site = correctionSite, Equipment = correctionEquipment,
            Quantity = 10, DateFrom = DateTime.UtcNow.AddDays(-5), DayPrice = 2 });
        await db.SaveChangesAsync();
        currentUser.Set(managerA.Id, "Kierownik Budowy");
        var correctionPz = await Send(new CreatePzCommand { FromConstructionSiteId = correctionSite.Id,
            Items = [new CreatePzItemDto { EquipmentId = correctionEquipment.Id, Quantity = 4 }] });
        List<ConfirmReturnItem> Receipt(int quantity, int scrap = 0) =>
            [new ConfirmReturnItem { EquipmentId = correctionEquipment.Id, Quantity = quantity, ScrappedQuantity = scrap }];
        await Denied(() => Confirm(correctionPz, Receipt(actual)), "manager cannot correct warehouse receipt");
        currentUser.Set(warehouse.Id, "Magazynier");
        await Denied(() => Confirm(correctionPz, Receipt(-1)), "negative receipt is rejected");
        await Denied(() => Confirm(correctionPz, Receipt(1, 2)), "receipt scrap cannot exceed actual quantity");
        await Denied(() => Confirm(correctionPz, Receipt(11)), "receipt cannot exceed declared plus remaining site stock");
        await Denied(() => Confirm(correctionPz, []), "receipt cannot omit declared items");
        db.ChangeTracker.Clear();
        Check((await db.Equipments.SingleAsync(x => x.Id == correctionEquipment.Id)).QuantityInTransit == 4 &&
            (await db.SiteEquipments.SingleAsync(x => x.SiteId == correctionSite.Id)).Quantity == 6,
            "rejected correction preserves pending stock");
        var scrap = actual > 0 ? 1 : 0;
        await Confirm(correctionPz, Receipt(actual, scrap));
        db.ChangeTracker.Clear();
        var corrected = await db.Equipments.SingleAsync(x => x.Id == correctionEquipment.Id);
        Check(corrected.QuantityAvailable == actual - scrap && corrected.QuantityScrapped == scrap &&
            corrected.QuantityInTransit == 0, "correction accepts actual usable quantity and scrap");
        Check((await db.SiteEquipments.SingleAsync(x => x.SiteId == correctionSite.Id)).Quantity == 10 - actual,
            "correction adjusts only difference on source site");
        var activeAllocations = await db.SiteEquipmentAllocations.Where(x => x.SiteId == correctionSite.Id && x.DateTo == null).ToListAsync();
        Check(activeAllocations.Sum(x => x.Quantity) == 10 - actual && activeAllocations.All(x => x.DayPrice == 2),
            "correction preserves allocation totals and original rates");
        var receiptDetails = await Send(new GetTransferByIdQuery(correctionPz));
        Check(receiptDetails.Items[0].Quantity == 4 && receiptDetails.Items[0].ReceivedQuantity == actual &&
            receiptDetails.Items[0].ReceivedScrappedQuantity == scrap, "receipt preserves declaration alongside actual result");
        await Denied(() => Confirm(correctionPz, Receipt(actual)), "corrected PZ cannot be accepted twice");
    }
    db.ChangeTracker.Clear();
    var historySite = new Site { Name = "Historical calculations" };
    db.SiteEquipmentAllocations.AddRange(
        new SiteEquipmentAllocation { Site = historySite, EquipmentId = equipment.Id, Quantity = 4,
            DateFrom = new DateTime(2020, 1, 1), DateTo = new DateTime(2020, 1, 6), DayPrice = 2 },
        new SiteEquipmentAllocation { Site = historySite, EquipmentId = equipment.Id, Quantity = 6,
            DateFrom = new DateTime(2020, 1, 1), DateTo = new DateTime(2020, 1, 11), DayPrice = 2 });
    await db.SaveChangesAsync();
    currentUser.Set(admin.Id, "Admin");
    var history = await Send(new GetAllHistoricalDataQuery { SiteId = historySite.Id, EquipmentId = equipment.Id });
    Check(history.Sum(x => x.TotalEarned) == 160, "partial returns stop earnings only for returned quantities");
    var filteredHistory = await Send(new GetAllHistoricalDataQuery { SiteId = historySite.Id, EquipmentId = equipment.Id,
        FromDate = new DateOnly(2020, 1, 3), ToDate = new DateOnly(2020, 1, 5) });
    Check(filteredHistory.Sum(x => x.TotalEarned) == 40 && filteredHistory.All(x => x.Days == 2),
        "equipment, site and date filters calculate earnings within selected dates");
    var futureHistory = await Send(new GetAllHistoricalDataQuery { SiteId = historySite.Id,
        FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)) });
    Check(futureHistory.Count == 0, "future date filter does not produce fictitious earnings");
    await Denied(async () => { await Send(new GetAllHistoricalDataQuery { FromDate = new DateOnly(2020, 2, 1),
        ToDate = new DateOnly(2020, 1, 1) }); }, "historical data rejects reversed date range");
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    await Denied(async () => { await Send(new GetAllHistoricalDataQuery { SiteId = historySite.Id }); },
        "manager cannot access historical earnings");
    await Denied(async () => { await Send(new GetAllHistoricalDataQuery { SiteId = siteA.Id }); },
        "manager cannot access earnings even for own site");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await Send(new GetAllHistoricalDataQuery()); },
        "warehouse cannot access historical earnings");
    currentUser.Set(admin.Id, "Admin");
    var initialSite = new Site { Name = "Initial stock earnings" };
    db.Sites.Add(initialSite);
    await db.SaveChangesAsync();
    await new Rogbau.Api.Features.Sites.Commands.StartSite.StartSiteCommandHandler(db, currentUser).Handle(
        new Rogbau.Api.Features.Sites.Commands.StartSite.StartSiteCommand { Id = initialSite.Id, StartDate = today.AddDays(1) }, CancellationToken.None);
    Check(initialSite.Status == SiteStatus.Planned, "saving future start leaves site planned");
    var scheduledList = await Send(new Rogbau.Api.Features.Sites.Queries.GetAllSites.GetAllQuery());
    Check(scheduledList.Single(s => s.Id == initialSite.Id).Status == SiteStatus.Planned, "site list shows future start as planned");
    async Task SetInitial(int quantity) => await new Rogbau.Api.Features.Sites.Commands.SetSiteInitialStock.SetSiteInitialStockCommandHandler(db, currentUser).Handle(new Rogbau.Api.Features.Sites.Commands.SetSiteInitialStock.SetSiteInitialStockCommand
    {
        SiteId = initialSite.Id,
        Items = [new Rogbau.Api.Features.Sites.Dtos.SetSiteInitialStockItemDto { EquipmentId = equipment.Id, Quantity = quantity }]
    }, CancellationToken.None);
    await SetInitial(10);
    var initialAllocation = await db.SiteEquipmentAllocations.SingleAsync(a => a.SiteId == initialSite.Id);
    Check(initialAllocation.Quantity == 10 && initialAllocation.DateFrom.Date == DateTime.UtcNow.Date,
        "initial stock starts earnings on entry date");
    await SetInitial(10);
    Check(await db.SiteEquipmentAllocations.CountAsync(a => a.SiteId == initialSite.Id) == 1,
        "saving unchanged stock does not duplicate history");
    await SetInitial(6);
    Check(await db.SiteEquipmentAllocations.Where(a => a.SiteId == initialSite.Id && a.DateTo == null).SumAsync(a => a.Quantity) == 6 &&
        await db.SiteEquipmentAllocations.Where(a => a.SiteId == initialSite.Id && a.DateTo != null).SumAsync(a => a.Quantity) == 4,
        "stock correction keeps partial historical quantities");
    var sameDay = await Send(new GetAllHistoricalDataQuery { SiteId = initialSite.Id });
    Check(sameDay.Count > 0 && sameDay.All(x => x.Days == 0 && x.TotalEarned == 0),
        "same-day stock corrections do not charge additional days and remain visible");
    await SetInitial(0);
    Check(!await db.SiteEquipmentAllocations.AnyAsync(a => a.SiteId == initialSite.Id && a.DateTo == null),
        "clearing initial stock closes earnings");
    var supplier = new Supplier { Name = "Test supplier" };
    db.Suppliers.Add(supplier);
    await db.SaveChangesAsync();
    Rogbau.Api.Features.Purchases.Commands.CreatePurchaseCommand Purchase(params int[] quantities) => new()
    {
        SupplierId = supplier.Id, PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Items = quantities.Select(q => new Rogbau.Api.Features.Purchases.Dtos.CreatePurchaseItemDto
            { EquipmentId = equipment.Id, Quantity = q, PurchasePrice = 8 }).ToList()
    };
    await Denied(async () => { await Send(Purchase(-1)); }, "purchase rejects negative quantities");
    await Denied(async () => { await Send(Purchase()); }, "purchase rejects empty items");
    await Denied(async () => { await Send(Purchase(1, 2)); }, "purchase rejects duplicate equipment");
    var before = await db.Equipments.AsNoTracking().SingleAsync(e => e.Id == equipment.Id);
    await Send(Purchase(3));
    var after = await db.Equipments.AsNoTracking().SingleAsync(e => e.Id == equipment.Id);
    Check(after.QuantityAvailable == before.QuantityAvailable + 3 && after.PurchasedQuantity == before.PurchasedQuantity + 3,
        "valid purchase increases warehouse stock exactly once");
    var pricedPurchase = Purchase(2);
    pricedPurchase.Items.Clear();
    pricedPurchase.Items.Add(new Rogbau.Api.Features.Purchases.Dtos.CreatePurchaseItemDto { EquipmentId = equipment.Id, Quantity = 2, PurchasePrice = 10 });
    var pricedId = await Send(pricedPurchase);
    Check(await db.Purchases.AsNoTracking().Where(p => p.Id == pricedId).Select(p => p.TotalPurchasePrice).SingleAsync() == 20,
        "purchase total uses submitted unit price");
    Check(await db.PurchaseItems.AsNoTracking().AnyAsync(p => p.EquipmentId == equipment.Id && p.Quantity == 3 && p.PurchasePrice == 8),
        "later purchase price preserves earlier purchase values");
    pricedPurchase.Items.Clear();
    pricedPurchase.Items.Add(new Rogbau.Api.Features.Purchases.Dtos.CreatePurchaseItemDto { EquipmentId = equipment.Id, Quantity = 2, PurchasePrice = -1 });
    await Denied(async () => { await Send(pricedPurchase); }, "purchase rejects negative unit price");
    await InvalidField(async () => { await Send(pricedPurchase); }, "Items[0].PurchasePrice");
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    await Denied(async () => { await Send(new CreateCategoryCommand { Name = "Forbidden" }); }, "manager cannot manage categories");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await Send(new CreateCategoryCommand { Name = "Forbidden" }); }, "warehouse cannot manage categories");
    currentUser.Set(admin.Id, "Admin");
    var categoryId = await Send(new CreateCategoryCommand { Name = "  New category  " });
    await InvalidField(async () => { await Send(new CreateCategoryCommand { Name = new string('a', 101) }); }, "Name");
    await InvalidField(async () => { await Send(new EditCategoryCommand { Id = categoryId, Name = "   " }); }, "Name");
    Check(await db.Categories.AsNoTracking().AnyAsync(c => c.Id == categoryId && c.Name == "New category"), "admin creates trimmed category");
    await Denied(async () => { await Send(new CreateCategoryCommand { Name = "new category" }); }, "duplicate category is rejected");
    var subId = await Send(new CreateSubCategoryCommand { Name = "New subcategory", CategoryId = categoryId });
    await Send(new EditCategoryCommand { Id = categoryId, Name = "Renamed category" });
    Check(await db.Categories.AsNoTracking().AnyAsync(c => c.Id == categoryId && c.Name == "Renamed category"), "separate category edit handler renames category");
    await Denied(async () => { await Send(new CreateSubCategoryCommand { Name = "new subcategory", CategoryId = categoryId }); }, "duplicate subcategory is rejected");
    await Denied(async () => { await Send(new CreateSubCategoryCommand { Name = "Orphan", CategoryId = Guid.NewGuid() }); }, "subcategory requires existing parent");
    await Send(new EditSubCategoryCommand { Id = subId, Name = "Renamed subcategory" });
    Check(await db.SubCategories.AsNoTracking().AnyAsync(c => c.Id == subId && c.Name == "Renamed subcategory" && c.CategoryId == categoryId), "subcategory rename retains parent");
    await Denied(async () => { await Send(new DeleteCategoryCommand { Id = categoryId }); }, "category with subcategories cannot be deleted");
    await Denied(async () => { await Send(new DeleteCategoryCommand { Id = category.Id }); }, "category used by equipment cannot be deleted");
    await Send(new DeleteSubCategoryCommand { Id = subId });
    await Send(new DeleteCategoryCommand { Id = categoryId });
    Check(!await db.Categories.AsNoTracking().AnyAsync(c => c.Id == categoryId), "unused category can be deleted");
    var managerSite = new Site { Name = "Manager lifecycle", Status = SiteStatus.Planned };
    db.Sites.Add(managerSite);
    db.SiteUsers.Add(new SiteUser { Site = managerSite, UserId = managerA.Id });
    await db.SaveChangesAsync();
    currentUser.Set(managerB.Id, "Kierownik Budowy");
    await Denied(async () => { await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.StartSite.StartSiteCommand { Id = managerSite.Id, StartDate = today }); },
        "unassigned manager cannot start site");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.StartSite.StartSiteCommand { Id = managerSite.Id, StartDate = today }); },
        "warehouse cannot start site");
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.StartSite.StartSiteCommand { Id = managerSite.Id, StartDate = today });
    Check(managerSite.Status == SiteStatus.InProgress, "assigned manager can start site");
    currentUser.Set(managerB.Id, "Kierownik Budowy");
    await Denied(async () => { await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.FinishSite.FinishSiteCommand { Id = managerSite.Id, EndDate = today }); },
        "unassigned manager cannot finish site");
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    await Denied(async () => { await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.FinishSite.FinishSiteCommand { Id = managerSite.Id, EndDate = today }); },
        "assigned manager cannot finish own site");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.FinishSite.FinishSiteCommand { Id = managerSite.Id, EndDate = today }); },
        "warehouse cannot finish site");
    currentUser.Set(admin.Id, "Admin");
    await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.FinishSite.FinishSiteCommand { Id = managerSite.Id, EndDate = today });
    Check(managerSite.Status == SiteStatus.Completed, "admin can finish empty site");
    var inventorySite = new Site { Name = "Manager inventory", Status = SiteStatus.InProgress };
    db.Sites.Add(inventorySite);
    db.SiteUsers.Add(new SiteUser { Site = inventorySite, UserId = managerA.Id });
    await db.SaveChangesAsync();
    async Task Inventory(int quantity) => await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Sites.Commands.SetSiteInitialStock.SetSiteInitialStockCommand {
            SiteId = inventorySite.Id, Items = [new Rogbau.Api.Features.Sites.Dtos.SetSiteInitialStockItemDto {
                EquipmentId = equipment.Id, Quantity = quantity }] });
    currentUser.Set(managerB.Id, "Kierownik Budowy");
    await Denied(async () => await Inventory(10), "unassigned manager cannot change initial stock");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => await Inventory(10), "warehouse cannot change initial stock");
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    await Inventory(10);
    await Inventory(6);
    Check(await db.SiteEquipments.AsNoTracking().Where(s => s.SiteId == inventorySite.Id).SumAsync(s => s.Quantity) == 6 &&
        await db.SiteEquipmentAllocations.AsNoTracking().Where(a => a.SiteId == inventorySite.Id && a.DateTo != null).SumAsync(a => a.Quantity) == 4,
        "assigned manager inventory correction preserves history");
    var sharedNotice = new Notification { Message = "Shared notification", ActionUrl = "/transfers" };
    var otherNotice = new Notification { Message = "Unread notification", ActionUrl = "/orders" };
    db.Recipients.AddRange(
        new NotificationRecipient { Notification = sharedNotice, UserId = managerA.Id },
        new NotificationRecipient { Notification = sharedNotice, UserId = managerB.Id },
        new NotificationRecipient { Notification = otherNotice, UserId = managerA.Id });
    await db.SaveChangesAsync();
    currentUser.Set(managerA.Id, "Kierownik Budowy");
    await Send(new Rogbau.Api.Features.Notifications.Commands.MarkAsRead.MarkAsReadCommand { NotificationId = sharedNotice.Id });
    Check(await db.Recipients.AsNoTracking().AnyAsync(r => r.NotificationId == sharedNotice.Id && r.UserId == managerA.Id && r.IsRead) &&
        await db.Recipients.AsNoTracking().AnyAsync(r => r.NotificationId == sharedNotice.Id && r.UserId == managerB.Id && !r.IsRead) &&
        await db.Recipients.AsNoTracking().AnyAsync(r => r.NotificationId == otherNotice.Id && !r.IsRead),
        "reading notification affects only chosen notice and current recipient");
    await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Notifications.Commands.DeleteNotification.DeleteNotificationCommand(sharedNotice.Id));
    Check(!await db.Recipients.AsNoTracking().AnyAsync(r => r.NotificationId == sharedNotice.Id && r.UserId == managerA.Id) &&
        await db.Recipients.AsNoTracking().AnyAsync(r => r.NotificationId == sharedNotice.Id && r.UserId == managerB.Id),
        "deleting notification leaves other user's notification intact");
    try {
        await Send(new Rogbau.Api.Features.Notifications.Commands.MarkAsRead.MarkAsReadCommand { NotificationId = sharedNotice.Id });
        throw new Exception("Deleted notice was still accessible");
    } catch (KeyNotFoundException) { Check(true, "user cannot modify another recipient's notification"); }
    // Dashboard checks use this run's isolated database and real SQL queries.
    var dashboardManager = await NewUser("dashboardManager", "Kierownik Budowy");
    var emptyManager = await NewUser("emptyDashboardManager", "Kierownik Budowy");
    var dashboardSiteA = new Site { Name = "Dashboard A", Status = SiteStatus.InProgress };
    var dashboardSiteB = new Site { Name = "Dashboard B", Status = SiteStatus.InProgress };
    db.Sites.AddRange(dashboardSiteA, dashboardSiteB);
    db.SiteUsers.AddRange(new SiteUser { Site = dashboardSiteA, UserId = dashboardManager.Id },
        new SiteUser { Site = dashboardSiteB, UserId = dashboardManager.Id });
    db.SiteEquipments.AddRange(new SiteEquipment { Site = dashboardSiteA, EquipmentId = equipment.Id, Quantity = 11 },
        new SiteEquipment { Site = dashboardSiteB, EquipmentId = equipment.Id, Quantity = 22 });
    var dashboardLow = new Equipment { Name = "Dashboard low", CategoryId = equipment.CategoryId, QuantityAvailable = 2 };
    var dashboardInactive = new Equipment { Name = "Dashboard inactive", CategoryId = equipment.CategoryId, QuantityAvailable = 1, IsActive = false };
    db.Equipments.AddRange(dashboardLow, dashboardInactive);
    var dashboardNow = DateTime.UtcNow;
    var dashboardMonth = new DateTime(dashboardNow.Year, dashboardNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    EquipmentMovement DashboardMovement(string number, Site site, EquipmentMovementType type,
        EquipmentMovementStatus status, DateTime date) => new()
    {
        Number = number, Type = type, Status = status, CreatedAt = date, AddedByUserId = admin.Id,
        ToConstructionSiteId = type == EquipmentMovementType.WZ ? site.Id : null,
        FromConstructionSiteId = type == EquipmentMovementType.PZ ? site.Id : null
    };
    var dashboardWz = DashboardMovement("DASH-WZ", dashboardSiteA, EquipmentMovementType.WZ, EquipmentMovementStatus.Completed, dashboardNow);
    db.EquipmentMovements.AddRange(dashboardWz,
        DashboardMovement("DASH-PZ", dashboardSiteA, EquipmentMovementType.PZ, EquipmentMovementStatus.InTransit, dashboardNow),
        DashboardMovement("DASH-DRAFT", dashboardSiteA, EquipmentMovementType.WZ, EquipmentMovementStatus.Draft, dashboardNow),
        DashboardMovement("DASH-CANCELLED", dashboardSiteA, EquipmentMovementType.WZ, EquipmentMovementStatus.Cancelled, dashboardNow),
        DashboardMovement("DASH-OLD", dashboardSiteA, EquipmentMovementType.WZ, EquipmentMovementStatus.Completed, dashboardMonth.AddSeconds(-1)),
        DashboardMovement("DASH-FUTURE", dashboardSiteA, EquipmentMovementType.WZ, EquipmentMovementStatus.Completed, dashboardMonth.AddMonths(1)),
        DashboardMovement("DASH-OTHER", dashboardSiteB, EquipmentMovementType.WZ, EquipmentMovementStatus.Completed, dashboardNow));
    await db.SaveChangesAsync();

    currentUser.Set(admin.Id, "Admin");
    var adminDashboard = (AdminDashboardDto)await Send(new GetDashboardQuery());
    Check(adminDashboard.EquipmentAvailable == await db.Equipments.Where(e => e.IsActive).SumAsync(e => e.QuantityAvailable),
        "admin dashboard sums active warehouse stock");
    Check(adminDashboard.EquipmentsOnAllSites == await db.SiteEquipments.SumAsync(s => s.Quantity),
        "admin dashboard sums site stock");
    Check(adminDashboard.LowEquipmentStocks.Any(e => e.Id == dashboardLow.Id) &&
        adminDashboard.LowEquipmentStocks.All(e => e.Id != dashboardInactive.Id && e.Quantity <= 5),
        "dashboard low stock excludes inactive equipment");
    Check(adminDashboard.RecentlyDocuments.Count <= 5 && adminDashboard.RecentlyOrders.Count <= 5,
        "dashboard recent lists are limited");
    var json = JsonSerializer.Serialize<object>(adminDashboard, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    Check(json.Contains("\"dashboardType\":\"admin\"") && json.Contains("\"activeSiteQuantity\""),
        "dashboard serializes concrete role DTO fields");

    currentUser.Set(warehouse.Id, "Magazynier");
    var warehouseDashboard = (WarehousemanDashboardDto)await Send(new GetDashboardQuery());
    Check(warehouseDashboard.EquipmentAvailable == adminDashboard.EquipmentAvailable && warehouseDashboard.DashboardType == "warehouseman",
        "warehouse user receives warehouse DTO");

    currentUser.Set(dashboardManager.Id, "Kierownik Budowy");
    var managerDashboard = (SiteManagerDashboardDto)await Send(new GetDashboardQuery());
    Check(managerDashboard.ManagerSites.Count == 2 && managerDashboard.SelectedSiteId == dashboardSiteA.Id,
        "manager dashboard lists assigned sites and selects first site");
    Check(managerDashboard.SiteEquipmentQuantity == 11 && managerDashboard.WzQuantityThisMonth == 1 && managerDashboard.PzQuantityThisMonth == 1,
        "manager counts selected site's stock and monthly non-draft non-cancelled documents");
    Check(managerDashboard.RecentlyDocuments.All(d => d.From == dashboardSiteA.Name || d.To == dashboardSiteA.Name),
        "manager recent documents are restricted to selected site");
    var switchedDashboard = (SiteManagerDashboardDto)await Send(new GetDashboardQuery { SiteId = dashboardSiteB.Id });
    Check(switchedDashboard.SelectedSiteId == dashboardSiteB.Id && switchedDashboard.SiteEquipmentQuantity == 22 && switchedDashboard.WzQuantityThisMonth == 1,
        "manager can switch to another assigned site");
    await Denied(async () => { await Send(new GetDashboardQuery { SiteId = siteC.Id }); },
        "manager dashboard rejects unassigned site");
    currentUser.Set(emptyManager.Id, "Kierownik Budowy");
    var emptyDashboard = (SiteManagerDashboardDto)await Send(new GetDashboardQuery());
    Check(emptyDashboard.ManagerSites.Count == 0 && emptyDashboard.SelectedSiteId == null && emptyDashboard.Status == null && emptyDashboard.RecentlyDocuments.Count == 0,
        "manager without sites receives empty dashboard");
    currentUser.Set(emptyManager.Id, "UnknownRole");
    await Denied(async () => { await Send(new GetDashboardQuery()); }, "dashboard rejects unsupported role");

    // Read access is protected in API metadata as well as the React menu.
    foreach (var restricted in new[] {
        (Controller: typeof(Rogbau.Api.Features.HistoricalData.HistoricalDataController), AdminOnly: true),
        (Controller: typeof(Rogbau.Api.Features.Purchases.PurchaseController), AdminOnly: false),
        (Controller: typeof(Rogbau.Api.Features.Suppliers.SuppliersController), AdminOnly: false) })
    {
        var attributes = restricted.Controller.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()
            .Concat(restricted.Controller.GetMethod("GetAll")!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        foreach (var attribute in attributes)
            if (!string.IsNullOrEmpty(attribute.Roles)) policyBuilder.RequireRole(attribute.Roles.Split(',').Select(role => role.Trim()));
        var policy = policyBuilder.Build();
        foreach (var role in new[] { "Admin", "Magazynier", "Kierownik Budowy" })
        {
            var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)], "Test"));
            var expected = role == "Admin" || (role == "Magazynier" && !restricted.AdminOnly);
            Check((await setup.ServiceProvider.GetRequiredService<IAuthorizationService>().AuthorizeAsync(principal, null, policy)).Succeeded == expected,
                restricted.Controller.Name + " read permission for " + role);
        }
        Check(!(await setup.ServiceProvider.GetRequiredService<IAuthorizationService>().AuthorizeAsync(
            new System.Security.Claims.ClaimsPrincipal(), null, policy)).Succeeded,
            restricted.Controller.Name + " rejects anonymous read");
    }

    // Regression checks for the handover fixes.
    var handoverManager = await NewUser("handoverManager", "Kierownik Budowy");
    var incomingSite = new Site { Name = "Handover incoming", Status = SiteStatus.InProgress };
    var sourceSite = new Site { Name = "Handover source", Status = SiteStatus.InProgress };
    var targetSite = new Site { Name = "Handover target", Status = SiteStatus.InProgress };
    var handoverEquipment = new Equipment { Name = "Handover equipment", CategoryId = equipment.CategoryId,
        InitialQuantity = 13, QuantityAvailable = 10, DayPrice = 1, PurchasePrice = 10 };
    db.AddRange(incomingSite, sourceSite, targetSite, handoverEquipment);
    db.SiteUsers.AddRange(new SiteUser { Site = incomingSite, UserId = handoverManager.Id },
        new SiteUser { Site = sourceSite, UserId = handoverManager.Id },
        new SiteUser { Site = targetSite, UserId = handoverManager.Id });
    db.SiteEquipments.Add(new SiteEquipment { Site = sourceSite, Equipment = handoverEquipment, Quantity = 3 });
    await db.SaveChangesAsync();
    CreateWzCommand HandoverWz(Guid siteId) => new()
    {
        ToConstructionSiteId = siteId,
        Items = [new CreateWzItemDto { EquipmentId = handoverEquipment.Id, Quantity = 1 }]
    };
    currentUser.Set(warehouse.Id, "Magazynier");
    handoverEquipment.IsActive = false;
    await db.SaveChangesAsync();
    await Denied(async () => { await Send(HandoverWz(incomingSite.Id)); }, "WZ rejects inactive equipment");
    Check((await db.Equipments.AsNoTracking().SingleAsync(e => e.Id == handoverEquipment.Id)).QuantityAvailable == 10,
        "rejected inactive WZ preserves warehouse stock");
    handoverEquipment.IsActive = true;
    await db.SaveChangesAsync();
    var incomingWz = await Send(HandoverWz(incomingSite.Id));
    currentUser.Set(handoverManager.Id, "Kierownik Budowy");
    var finishIncoming = new Rogbau.Api.Features.Sites.Commands.FinishSite.FinishSiteCommand
    { Id = incomingSite.Id, EndDate = DateOnly.FromDateTime(DateTime.UtcNow) };
    currentUser.Set(admin.Id, "Admin");
    await Denied(async () => { await Send(finishIncoming); }, "site cannot finish with incoming transport");
    currentUser.Set(handoverManager.Id, "Kierownik Budowy");
    await Confirm(incomingWz);
    currentUser.Set(handoverManager.Id, "Kierownik Budowy");
    var incomingPz = await Send(new CreatePzCommand { FromConstructionSiteId = incomingSite.Id,
        Items = [new CreatePzItemDto { EquipmentId = handoverEquipment.Id, Quantity = 1 }] });
    currentUser.Set(admin.Id, "Admin");
    await Denied(async () => { await Send(finishIncoming); }, "site cannot finish with pending return");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Confirm(incomingPz);
    currentUser.Set(admin.Id, "Admin");
    await Send(finishIncoming);
    Check(await db.Sites.AsNoTracking().AnyAsync(s => s.Id == incomingSite.Id && s.Status == SiteStatus.Completed),
        "site can finish after receipt and equipment return");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await Send(HandoverWz(incomingSite.Id)); }, "WZ rejects completed target site");
    currentUser.Set(handoverManager.Id, "Kierownik Budowy");
    await Denied(async () => { await Send(new CreateOrderCommand { SiteId = incomingSite.Id,
        Items = [new CreateOrderItemDto { EquipmentId = handoverEquipment.Id, Quantity = 1 }] }); },
        "order rejects completed site");
    CreateTransferCommand HandoverTransfer(Guid destination) => new()
    {
        FromConstructionSiteId = sourceSite.Id, ToConstructionSiteId = destination,
        Items = [new CreateTransferItemDto { EquipmentId = handoverEquipment.Id, Quantity = 1 }]
    };
    await Denied(async () => { await Send(HandoverTransfer(incomingSite.Id)); }, "transfer rejects completed target site");
    var ownTransfer = await Send(HandoverTransfer(targetSite.Id));
    Check((await Send(new GetTransferByIdQuery(ownTransfer))).CanConfirm,
        "manager assigned to target can confirm own transfer");
    await Confirm(ownTransfer);
    Check(await db.SiteEquipments.AsNoTracking().AnyAsync(s => s.SiteId == targetSite.Id && s.Quantity == 1),
        "own transfer receipt adds stock to target site");
    await Denied(async () => await Confirm(ownTransfer), "own transfer cannot be confirmed twice");
    var fallbackTransfer = await Send(HandoverTransfer(targetSite.Id));
    currentUser.Set(warehouse.Id, "Magazynier");
    Check(!(await Send(new GetTransferByIdQuery(fallbackTransfer))).CanConfirm, "warehouse has no fallback confirmation button");
    await Denied(async () => await Confirm(fallbackTransfer), "warehouse cannot use fallback confirmation");
    currentUser.Set(admin.Id, "Admin");
    Check((await Send(new GetTransferByIdQuery(fallbackTransfer))).CanConfirm, "admin sees fallback confirmation for another user's transfer");
    await Confirm(fallbackTransfer);
    Check(await db.SiteEquipments.AsNoTracking().AnyAsync(s => s.SiteId == targetSite.Id && s.Quantity == 2),
        "admin fallback adds stock to target site");
    await Denied(async () => await Confirm(fallbackTransfer), "fallback transfer cannot be confirmed twice");
    var adminTransfer = await Send(HandoverTransfer(targetSite.Id));
    Check(!(await Send(new GetTransferByIdQuery(adminTransfer))).CanConfirm, "admin cannot confirm own transfer");
    await Denied(async () => await Confirm(adminTransfer), "admin self-confirmation is rejected");

    // Use the application's actual JWT validation event, with a fresh scope for each request.
    async Task<string> IssueSessionToken(string id)
    {
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var sessionUser = await userManager.FindByIdAsync(id) ?? throw new Exception("Missing session test user");
        return await scope.ServiceProvider.GetRequiredService<IJwtTokenService>().GenerateTokenAsync(sessionUser);
    }
    async Task<bool> SessionValid(string token)
    {
        using var scope = provider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>>()
            .Get(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, options.TokenValidationParameters, out var securityToken);
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = scope.ServiceProvider };
        var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme("Bearer", "Bearer",
            typeof(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler));
        var validation = new Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext(httpContext, scheme, options)
        { Principal = principal, SecurityToken = securityToken };
        await options.Events.TokenValidated(validation);
        return validation.Result?.Failure == null;
    }
    var sessionUser = await NewUser("handoverSession", "Admin");
    var sessionToken = await IssueSessionToken(sessionUser.Id);
    Check(await SessionValid(sessionToken), "new JWT session is accepted");
    await setup.ServiceProvider.GetRequiredService<IMediator>().Send(
        new Rogbau.Api.Features.Users.Commands.ChangePasswordByAdmin.ChangePasswordByAdminCommand(sessionUser.Id, "HandoverTest!4829"));
    Check(!await SessionValid(sessionToken), "password reset invalidates existing JWT");
    sessionToken = await IssueSessionToken(sessionUser.Id);
    Check(await SessionValid(sessionToken), "fresh JWT after password reset is accepted");
    currentUser.Set(sessionUser.Id, "Admin");
    await Send(new Rogbau.Api.Features.Users.Commands.ChangePasswordByUser.ChangePasswordByUserCommand(
        "HandoverTest!4829", "HandoverChanged!4829"));
    Check(!await SessionValid(sessionToken), "own password change invalidates existing JWT");
    sessionToken = await IssueSessionToken(sessionUser.Id);
    currentUser.Set(admin.Id, "Admin");
    await Send(new Rogbau.Api.Features.Users.Commands.EditUser.EditUserCommand
    { Id = sessionUser.Id, Email = sessionUser.Email, FirstName = "Session", LastName = "Test", Role = "Magazynier" });
    Check(!await SessionValid(sessionToken), "role change invalidates existing JWT");
    sessionToken = await IssueSessionToken(sessionUser.Id);
    Check(await SessionValid(sessionToken), "new JWT contains current role and is accepted");
    await Send(new Rogbau.Api.Features.Users.Commands.DeleteUser.DeleteUserCommand(sessionUser.Id));
    Check(!await SessionValid(sessionToken), "deleted user cannot use an existing JWT");

    var usersBeforeSeed = await db.Users.CountAsync();
    await DbInitializer.SeedAsync(provider);
    Check(await db.Users.CountAsync() == usersBeforeSeed, "startup does not recreate default admin when users already exist");

    // Read the rate back from SQL Server, not from EF's tracked entity.
    var preciseSite = new Site { Name = "Precise daily rates" };
    db.SiteEquipmentAllocations.AddRange(
        new SiteEquipmentAllocation { Site = preciseSite, EquipmentId = equipment.Id, Quantity = 1000,
            DateFrom = new DateTime(2020, 1, 1), DateTo = new DateTime(2020, 1, 11), DayPrice = 0.0045m },
        new SiteEquipmentAllocation { Site = preciseSite, EquipmentId = equipment.Id, Quantity = 100,
            DateFrom = new DateTime(2020, 1, 1), DateTo = new DateTime(2020, 1, 11), DayPrice = 1.2345m });
    await db.SaveChangesAsync();
    var preciseHistory = await Send(new GetAllHistoricalDataQuery { SiteId = preciseSite.Id });
    Check(preciseHistory.Any(x => x.DayPrice == 0.0045m) && preciseHistory.Any(x => x.DayPrice == 1.2345m),
        "SQL history preserves four decimal places including sub-cent rates");
    Check(preciseHistory.Sum(x => x.TotalEarned) == 1279.5m,
        "earnings use full precision persisted daily rates");

    Rogbau.Api.Features.Users.Commands.EditUser.EditUserCommand EditRole(ApplicationUser account, string role) => new()
    { Id = account.Id, Email = account.Email, FirstName = "Unchanged", LastName = "Admin", Role = role };
    var soleAdminToken = await IssueSessionToken(admin.Id);
    var originalAdminName = await db.Users.AsNoTracking().Where(x => x.Id == admin.Id).Select(x => x.FirstName).SingleAsync();
    await Denied(async () => { await Send(EditRole(admin, "Magazynier")); }, "last admin cannot demote own account");
    Check(await db.UserRoles.AsNoTracking().AnyAsync(x => x.UserId == admin.Id &&
        db.Roles.Any(r => r.Id == x.RoleId && r.Name == "Admin")), "rejected demotion preserves admin role");
    Check(await db.Users.AsNoTracking().Where(x => x.Id == admin.Id).Select(x => x.FirstName).SingleAsync() == originalAdminName,
        "rejected demotion preserves account details");
    Check(await SessionValid(soleAdminToken), "rejected demotion preserves administrator session");
    currentUser.Set(warehouse.Id, "Magazynier");
    await Denied(async () => { await Send(new Rogbau.Api.Features.Users.Commands.DeleteUser.DeleteUserCommand(admin.Id)); },
        "last administrator cannot be deleted by handler");
    currentUser.Set(admin.Id, "Admin");
    var backupAdmin = await NewUser("backupAdmin", "Admin");
    await Send(EditRole(admin, "Magazynier"));
    Check(!await SessionValid(soleAdminToken), "demotion with another admin succeeds and invalidates old session");
    currentUser.Set(backupAdmin.Id, "Admin");
    await Send(EditRole(admin, "Admin"));
    currentUser.Set(admin.Id, "Admin");
    await Send(new Rogbau.Api.Features.Users.Commands.DeleteUser.DeleteUserCommand(backupAdmin.Id));
    Check(!await db.Users.AsNoTracking().AnyAsync(x => x.Id == backupAdmin.Id), "additional administrator can be deleted");
    await Send(EditRole(admin, "Admin"));
    Check(await db.UserRoles.AsNoTracking().AnyAsync(x => x.UserId == admin.Id &&
        db.Roles.Any(r => r.Id == x.RoleId && r.Name == "Admin")), "sole admin can still edit account without changing role");

    var concurrentAdmin = await NewUser("concurrentAdmin", "Admin");
    async Task TryDemote(ApplicationUser account)
    {
        try { await Send(EditRole(account, "Magazynier")); }
        catch (Rogbau.Api.Common.Exceptions.ConflictException) { }
        catch (SqlException e) when (e.Number == 1205) { } // SQL aborts one competing serializable transaction.
    }
    await Task.WhenAll(TryDemote(admin), TryDemote(concurrentAdmin));
    Check(await db.UserRoles.AsNoTracking().CountAsync(x => db.Roles.Any(r => r.Id == x.RoleId && r.Name == "Admin")) >= 1,
        "concurrent demotions cannot remove all administrators");
    Console.WriteLine($"All {passed} workflow checks passed.");
}
finally
{
    if (created)
    {
        using var cleanup = provider.CreateScope();
        var db = cleanup.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog != testDatabase)
            throw new Exception("Refusing to remove a database other than this test run's database.");
        await db.Database.EnsureDeletedAsync();
        Console.WriteLine("Temporary test database removed.");
    }
}

class TestUser : ICurrentUserService
{
    public string? UserId { get; private set; }
    public string? Email => null;
    public bool IsAuthenticated => UserId != null;
    private string role = "";
    public void Set(string id, string name) { UserId = id; role = name; }
    public bool IsInRole(string name) => role == name;
    public IEnumerable<string> GetRoles() => [role];
}

class TestHub : IHubContext<NotificationHub>
{
    public static List<(IReadOnlyList<string> Ids, string Method)> Deliveries { get; } = [];
    public IHubClients Clients { get; } = new TestClients();
    public IGroupManager Groups => throw new NotSupportedException();
}
class TestProxy(IReadOnlyList<string> ids) : IClientProxy
{
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    { TestHub.Deliveries.Add((ids, method)); return Task.CompletedTask; }
}
class TestClients : IHubClients
{
    public IClientProxy Users(IReadOnlyList<string> ids) => new TestProxy(ids);
    public IClientProxy User(string id) => new TestProxy([id]);
    public IClientProxy All => new TestProxy([]);
    public IClientProxy AllExcept(IReadOnlyList<string> ids) => throw new NotSupportedException();
    public IClientProxy Client(string id) => throw new NotSupportedException();
    public IClientProxy Clients(IReadOnlyList<string> ids) => throw new NotSupportedException();
    public IClientProxy Group(string name) => throw new NotSupportedException();
    public IClientProxy GroupExcept(string name, IReadOnlyList<string> ids) => throw new NotSupportedException();
    public IClientProxy Groups(IReadOnlyList<string> names) => throw new NotSupportedException();
}
