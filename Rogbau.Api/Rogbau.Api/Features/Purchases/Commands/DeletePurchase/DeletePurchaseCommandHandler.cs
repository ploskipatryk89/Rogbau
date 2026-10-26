using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;
using System.Data;

namespace Rogbau.Api.Features.Purchases.Commands.DeletePurchase
{
    public class DeletePurchaseCommandHandler : IRequestHandler<DeletePurchaseCommand, Unit>
    {
        private readonly ApplicationDbContext _context;

        public DeletePurchaseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
    DeletePurchaseCommand request,
    CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var purchase = await _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(item => item.Equipment)
                .FirstOrDefaultAsync(
                    p => p.Id == request.Id,
                    cancellationToken);

            if (purchase == null)
            {
                throw new KeyNotFoundException(
                    "Nie znaleziono faktury zakupu.");
            }

            foreach (var item in purchase.Items)
            {
                var equipment = item.Equipment;

                if (equipment.QuantityAvailable < item.Quantity ||
                    equipment.PurchasedQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Nie można usunąć faktury. " +
                        $"Nie można cofnąć zakupu sprzętu: {equipment.Name}.");
                }

                equipment.QuantityAvailable -= item.Quantity;
                equipment.PurchasedQuantity -= item.Quantity;
            }

            _context.PurchaseItems.RemoveRange(purchase.Items);
            _context.Purchases.Remove(purchase);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
