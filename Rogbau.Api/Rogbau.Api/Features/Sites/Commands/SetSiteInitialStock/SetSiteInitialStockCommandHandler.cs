using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Commands.SetSiteInitialStock
{
    public class SetSiteInitialStockCommandHandler : IRequestHandler<SetSiteInitialStockCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly Rogbau.Api.Services.ICurrentUserService _user;

        public SetSiteInitialStockCommandHandler(ApplicationDbContext context, Rogbau.Api.Services.ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task Handle(SetSiteInitialStockCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin") && !(_user.IsInRole("Kierownik Budowy") &&
                await _context.SiteUsers.AnyAsync(s => s.SiteId == request.SiteId && s.UserId == _user.UserId, cancellationToken)))
                throw new UnauthorizedAccessException("Możesz ustalać bilans tylko przypisanych do Ciebie budów.");
            if (request.Items.Any(i => i.Quantity < 0) ||
                request.Items.Select(i => i.EquipmentId).Distinct().Count() != request.Items.Count)
                throw new ArgumentException("Ilości nie mogą być ujemne, a sprzęt nie może się powtarzać.");

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var siteExists = await _context.Sites
                .AnyAsync(s => s.Id == request.SiteId, cancellationToken);

            if (!siteExists)
            {
                throw new KeyNotFoundException($"Nie znaleziono budowy o Id {request.SiteId}");
            }
            if (await _context.Sites.AnyAsync(s => s.Id == request.SiteId &&
                s.Status == Rogbau.Api.Models.Enums.SiteStatus.Completed, cancellationToken))
                throw new InvalidOperationException("Nie można zmieniać stanu zakończonej budowy.");

            var existingRecords = await _context.SiteEquipments
            .Where(se => se.SiteId == request.SiteId)
            .ToListAsync(cancellationToken);

            var equipmentIds = request.Items.Select(i => i.EquipmentId).ToList();
            var equipment = await _context.Equipments.Where(e => equipmentIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, cancellationToken);
            if (equipment.Count != equipmentIds.Count)
                throw new ArgumentException("Nie znaleziono wybranego sprzętu.");

            var allocations = await _context.SiteEquipmentAllocations
                .Where(a => a.SiteId == request.SiteId && a.DateTo == null)
                .OrderBy(a => a.DateFrom).ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;
            // Zachowujemy wcześniejsze okresy. Zmiana stanu dotyczy naliczania od teraz.
            foreach (var equipmentId in equipmentIds.Concat(allocations.Select(a => a.EquipmentId)).Distinct())
            {
                var target = request.Items.FirstOrDefault(i => i.EquipmentId == equipmentId)?.Quantity ?? 0;
                var active = allocations.Where(a => a.EquipmentId == equipmentId).ToList();
                var difference = target - active.Sum(a => a.Quantity);
                if (difference > 0)
                    _context.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation
                    {
                        SiteId = request.SiteId, EquipmentId = equipmentId, Quantity = difference,
                        DateFrom = now, DayPrice = equipment[equipmentId].DayPrice
                    });
                else if (difference < 0)
                {
                    var remaining = -difference;
                    foreach (var allocation in active)
                    {
                        var closed = Math.Min(remaining, allocation.Quantity);
                        if (closed == allocation.Quantity) allocation.DateTo = now;
                        else
                        {
                            allocation.Quantity -= closed;
                            _context.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation
                            {
                                SiteId = request.SiteId, EquipmentId = equipmentId, Quantity = closed,
                                DateFrom = allocation.DateFrom, DateTo = now, DayPrice = allocation.DayPrice
                            });
                        }
                        remaining -= closed;
                        if (remaining == 0) break;
                    }
                }
            }

            if (existingRecords.Count != 0)
            {
                _context.SiteEquipments.RemoveRange(existingRecords);
            }

            // 3. Dodanie tylko pozycji z ilością > 0
            var newRecords = request.Items
                .Where(i => i.Quantity > 0)
                .Select(i => new SiteEquipment
                {
                    SiteId = request.SiteId,
                    EquipmentId = i.EquipmentId,
                    Quantity = i.Quantity
                });

            await _context.SiteEquipments.AddRangeAsync(newRecords, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
