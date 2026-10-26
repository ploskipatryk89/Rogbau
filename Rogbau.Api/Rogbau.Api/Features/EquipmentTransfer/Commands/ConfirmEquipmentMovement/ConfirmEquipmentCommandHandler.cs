using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.ConfirmEquipmentMovement
{
    public class ConfirmEquipmentMovementCommandHandler
        : IRequestHandler<ConfirmEquipmentMovementCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ConfirmEquipmentMovementCommandHandler(
            ApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(
            ConfirmEquipmentMovementCommand request,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var movement = await _context.EquipmentMovements
                .Include(x => x.Items)
                    .ThenInclude(x => x.Equipment)
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

            if (movement is null)
            {
                throw new InvalidOperationException("Nie znaleziono dokumentu.");
            }

            if (movement.Status != EquipmentMovementStatus.InTransit)
            {
                throw new InvalidOperationException(
                    "Dokument nie oczekuje na potwierdzenie.");
            }

            if (movement.Type != EquipmentMovementType.WZ &&
                movement.Type != EquipmentMovementType.Transfer && movement.Type != EquipmentMovementType.PZ)
            {
                throw new InvalidOperationException(
                    "Ten typ dokumentu nie wymaga potwierdzenia.");
            }

            if (movement.ToConstructionSiteId is null && movement.Type != EquipmentMovementType.PZ)
            {
                throw new InvalidOperationException("Brak budowy docelowej.");
            }

            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "Użytkownik nie jest zalogowany.");
            }

            if (movement.Type == EquipmentMovementType.PZ)
            {
                if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Magazynier"))
                    throw new UnauthorizedAccessException("Zwrot może przyjąć tylko administrator lub magazynier.");

                if (request.Items != null && (request.Items.Count != movement.Items.Count ||
                    request.Items.Select(x => x.EquipmentId).Distinct().Count() != request.Items.Count ||
                    request.Items.Any(x => !movement.Items.Any(item => item.EquipmentId == x.EquipmentId))))
                    throw new ArgumentException("Przyjęcie musi zawierać wszystkie pozycje zgłoszonego zwrotu.");

                foreach (var item in movement.Items)
                {
                    var receipt = request.Items?.Single(x => x.EquipmentId == item.EquipmentId);
                    var receivedQuantity = receipt?.Quantity ?? item.Quantity;
                    var receivedScrapped = receipt?.ScrappedQuantity ?? item.ScrappedQuantity;
                    if (item.Quantity <= 0 || item.ScrappedQuantity < 0 || item.ScrappedQuantity > item.Quantity ||
                        item.Equipment.QuantityInTransit < item.Quantity)
                        throw new InvalidOperationException("Nieprawidłowy stan zwracanego sprzętu.");
                    await CorrectReturnQuantity(movement, item, receivedQuantity, cancellationToken);
                    item.Equipment.QuantityInTransit -= item.Quantity;
                    item.Equipment.QuantityAvailable += receivedQuantity - receivedScrapped;
                    item.Equipment.QuantityScrapped += receivedScrapped;
                    item.ReceivedQuantity = receivedQuantity;
                    item.ReceivedScrappedQuantity = receivedScrapped;
                }
                movement.Status = EquipmentMovementStatus.Completed;
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            if (request.Items != null)
                throw new ArgumentException("Korekta ilości przy odbiorze jest dostępna tylko dla PZ.");

            if (movement.ToConstructionSiteId is null)
                throw new InvalidOperationException("Brak budowy docelowej.");

            var isManager =
                _currentUserService.IsInRole("Kierownik Budowy");

            var isAssignedToTargetSite = await _context.SiteUsers
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.SiteId ==
                            movement.ToConstructionSiteId.Value &&
                        x.UserId == userId,
                    cancellationToken);

            var canConfirm = isManager && isAssignedToTargetSite;
            if (_currentUserService.IsInRole("Admin") && movement.Type == EquipmentMovementType.Transfer &&
                movement.AddedByUserId != userId)
                canConfirm = true;

            if (!canConfirm)
            {
                throw new UnauthorizedAccessException(
                    "Nie masz uprawnień do potwierdzenia tego dokumentu.");
            }

            if (await _context.Sites.AnyAsync(s => s.Id == movement.ToConstructionSiteId &&
                s.Status == SiteStatus.Completed, cancellationToken))
                throw new InvalidOperationException("Nie można przyjąć sprzętu na zakończoną budowę.");

            foreach (var item in movement.Items)
            {
                if (item.Equipment.QuantityInTransit < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Nieprawidłowa ilość sprzętu w transporcie: {item.Equipment.Name}");
                }

                item.Equipment.QuantityInTransit -= item.Quantity;

                var siteEquipment = await _context.SiteEquipments
                    .FirstOrDefaultAsync(
                        x =>
                            x.SiteId ==
                                movement.ToConstructionSiteId.Value &&
                            x.EquipmentId == item.EquipmentId,
                        cancellationToken);

                if (siteEquipment is null)
                {
                    siteEquipment = new SiteEquipment
                    {
                        SiteId =
                            movement.ToConstructionSiteId.Value,

                        EquipmentId =
                            item.EquipmentId,

                        Quantity =
                            item.Quantity
                    };

                    _context.SiteEquipments.Add(siteEquipment);
                }
                else
                {
                    siteEquipment.Quantity += item.Quantity;
                }

                var allocation = new SiteEquipmentAllocation
                {
                    SiteId =
                        movement.ToConstructionSiteId.Value,

                    EquipmentId =
                        item.EquipmentId,

                    Quantity =
                        item.Quantity,

                    DateFrom =
                        DateTime.UtcNow,

                    DateTo =
                        null,

                    DayPrice =
                        item.Equipment.DayPrice
                };

                _context.SiteEquipmentAllocations.Add(allocation);
            }

            movement.Status =
                EquipmentMovementStatus.Completed;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private async Task CorrectReturnQuantity(EquipmentMovement movement, EquipmentMovementItem item,
            int receivedQuantity, CancellationToken cancellationToken)
        {
            var difference = receivedQuantity - item.Quantity;
            if (difference == 0) return;
            if (!movement.FromConstructionSiteId.HasValue)
                throw new InvalidOperationException("Brak budowy źródłowej zwrotu.");
            var siteId = movement.FromConstructionSiteId.Value;
            var stock = await _context.SiteEquipments.SingleOrDefaultAsync(x =>
                x.SiteId == siteId && x.EquipmentId == item.EquipmentId, cancellationToken);

            if (difference < 0)
            {
                var remaining = -difference;
                if (stock == null)
                {
                    stock = new SiteEquipment { SiteId = siteId, EquipmentId = item.EquipmentId };
                    _context.SiteEquipments.Add(stock);
                }
                stock.Quantity += remaining;

                // Przywracamy naliczanie od zgłoszenia zwrotu dla sztuk, które zostały na budowie.
                var closed = await _context.SiteEquipmentAllocations.Where(x => x.SiteId == siteId &&
                    x.EquipmentId == item.EquipmentId && x.DateTo == movement.CreatedAt)
                    .OrderByDescending(x => x.DateFrom).ToListAsync(cancellationToken);
                foreach (var allocation in closed)
                {
                    if (remaining == 0) break;
                    var quantity = Math.Min(remaining, allocation.Quantity);
                    _context.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation
                    {
                        SiteId = siteId, EquipmentId = item.EquipmentId, Quantity = quantity,
                        DateFrom = movement.CreatedAt, DayPrice = allocation.DayPrice
                    });
                    remaining -= quantity;
                }
                if (remaining > 0)
                    _context.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation
                    {
                        SiteId = siteId, EquipmentId = item.EquipmentId, Quantity = remaining,
                        DateFrom = movement.CreatedAt, DayPrice = item.Equipment.DayPrice
                    });
            }
            else
            {
                if (stock == null || stock.Quantity < difference)
                    throw new InvalidOperationException("Przyjęta ilość przekracza dostępny stan sprzętu na budowie.");
                var allocations = await _context.SiteEquipmentAllocations.Where(x => x.SiteId == siteId &&
                    x.EquipmentId == item.EquipmentId && x.DateTo == null)
                    .OrderBy(x => x.DateFrom).ToListAsync(cancellationToken);
                if (allocations.Sum(x => x.Quantity) > stock.Quantity)
                    throw new InvalidOperationException("Historia sprzętu przekracza stan budowy.");
                var remaining = difference;
                var now = DateTime.UtcNow;
                foreach (var allocation in allocations)
                {
                    if (remaining == 0) break;
                    var quantity = Math.Min(remaining, allocation.Quantity);
                    if (quantity == allocation.Quantity) allocation.DateTo = now;
                    else
                    {
                        allocation.Quantity -= quantity;
                        _context.SiteEquipmentAllocations.Add(new SiteEquipmentAllocation
                        {
                            SiteId = siteId, EquipmentId = item.EquipmentId, Quantity = quantity,
                            DateFrom = allocation.DateFrom, DateTo = now, DayPrice = allocation.DayPrice
                        });
                    }
                    remaining -= quantity;
                }
                stock.Quantity -= difference;
                if (stock.Quantity == 0) _context.SiteEquipments.Remove(stock);
            }
        }
    }
}
