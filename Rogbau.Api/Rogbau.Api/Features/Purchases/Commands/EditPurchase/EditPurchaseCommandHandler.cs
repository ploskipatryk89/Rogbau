using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Purchases.Commands.EditPurchase;

public class EditPurchaseCommandHandler(ApplicationDbContext context) : IRequestHandler<EditPurchaseCommand, Unit>
{
    public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);
        var purchase = await context.Purchases.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Nie znaleziono zakupu.");
        if (!await context.Suppliers.AnyAsync(s => s.Id == request.SupplierId, cancellationToken))
            throw new KeyNotFoundException("Nie znaleziono dostawcy.");
        var ids = purchase.Items.Select(i => i.EquipmentId).Concat(request.Items.Select(i => i.EquipmentId)).Distinct().ToList();
        var equipments = await context.Equipments.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
        if (equipments.Count != ids.Count) throw new KeyNotFoundException("Nie znaleziono sprzętu.");
        var others = await context.PurchaseItems.AsNoTracking()
            .Where(i => i.PurchaseId != purchase.Id && ids.Contains(i.EquipmentId)).ToListAsync(cancellationToken);
        foreach (var equipment in equipments)
        {
            var oldQuantity = purchase.Items.Where(i => i.EquipmentId == equipment.Id).Sum(i => i.Quantity);
            var newItems = request.Items.Where(i => i.EquipmentId == equipment.Id).ToList();
            var newQuantity = newItems.Sum(i => i.Quantity);
            var delta = newQuantity - oldQuantity;
            var oldCost = purchase.Items.Where(i => i.EquipmentId == equipment.Id).Sum(i => i.Quantity * i.PurchasePrice);
            var newCost = newItems.Sum(i => i.Quantity * i.PurchasePrice);
            if (delta == 0 && oldCost == newCost) continue;
            if (!equipment.IsActive && newQuantity > oldQuantity)
                throw new ArgumentException($"Nie można zwiększyć zakupu nieaktywnego sprzętu: {equipment.Name}.");
            if ((long)equipment.QuantityAvailable + delta < 0 || (long)equipment.PurchasedQuantity + delta < 0)
                throw new InvalidOperationException($"Nie można zmniejszyć zakupu sprzętu {equipment.Name}: brak wystarczającej ilości w magazynie.");
            equipment.QuantityAvailable = checked(equipment.QuantityAvailable + delta);
            equipment.PurchasedQuantity = checked(equipment.PurchasedQuantity + delta);
            var remaining = others.Where(i => i.EquipmentId == equipment.Id).ToList();
            var quantity = remaining.Sum(i => (long)i.Quantity) + newQuantity;
           
        }
        context.PurchaseItems.RemoveRange(purchase.Items);
        purchase.Items = request.Items.Select(i => new PurchaseItem
        {
            EquipmentId = i.EquipmentId, Quantity = i.Quantity, PurchasePrice = i.PurchasePrice
        }).ToList();
        context.PurchaseItems.AddRange(purchase.Items);
        purchase.SupplierId = request.SupplierId;
        purchase.PurchaseDate = request.PurchaseDate;
        purchase.TotalQuantity = request.Items.Sum(i => i.Quantity);
        purchase.TotalPurchasePrice = request.Items.Sum(i => i.Quantity * i.PurchasePrice);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}
