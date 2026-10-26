using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Purchases.Commands
{
    public class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreatePurchaseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);

            if (supplier == null)
                throw new KeyNotFoundException($"Nie znaleziono dostawcy o ID {request.SupplierId}");

            var purchaseItems = new List<PurchaseItem>();

            foreach (var item in request.Items)
            {
                var equipment = await _context.Equipments
                    .FirstOrDefaultAsync(e => e.Id == item.EquipmentId, cancellationToken);

                if (equipment == null)
                    throw new KeyNotFoundException($"Nie znaleziono sprzętu o ID {item.EquipmentId}");

                if (!equipment.IsActive)
                    throw new ArgumentException("Nie można kupić nieaktywnego sprzętu.");

                equipment.PurchasedQuantity += item.Quantity;
                equipment.QuantityAvailable += item.Quantity;

                purchaseItems.Add(new PurchaseItem
                {
                    EquipmentId = item.EquipmentId,
                    Quantity = item.Quantity,
                    PurchasePrice = item.PurchasePrice,
                    

                });
            }

            var purchase = new Purchase
            {
                SupplierId = request.SupplierId,
                PurchaseDate = request.PurchaseDate,
                PurchaseNumber = $"NZ/{request.PurchaseDate:yyyyMMdd}/{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
                Items = purchaseItems,
                
                TotalPurchasePrice = purchaseItems.Sum(i => i.Quantity * i.PurchasePrice)
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return purchase.Id;
        }
    }
}
