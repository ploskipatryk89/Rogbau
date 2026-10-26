using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Features.Notifications.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreateTransfer
{
    public class CreateTransferCommandHandler
        : IRequestHandler<CreateTransferCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _userService;
        private readonly NumberGenerator _numberGenerator;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CreateTransferCommandHandler(
            ApplicationDbContext context,
            ICurrentUserService userService,
            NumberGenerator numberGenerator,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userService = userService;
            _numberGenerator = numberGenerator;
            _hubContext = hubContext;
        }

        public async Task<Guid> Handle(
            CreateTransferCommand request,
            CancellationToken cancellationToken)
        {


            var userId = _userService.UserId;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany.");

            if (!_userService.IsInRole("Admin") && !_userService.IsInRole("Magazynier") &&
                !await _context.SiteUsers.AnyAsync(x => x.SiteId == request.FromConstructionSiteId &&
                    x.UserId == userId, cancellationToken))
                throw new UnauthorizedAccessException("Możesz wysyłać sprzęt tylko ze swojej budowy.");


            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var targetSite = await _context.Sites.FirstOrDefaultAsync(s => s.Id == request.ToConstructionSiteId, cancellationToken);
            if (targetSite == null)
                throw new KeyNotFoundException("Nie znaleziono budowy docelowej.");
            if (targetSite.Status == SiteStatus.Completed)
                throw new InvalidOperationException("Nie można przesunąć sprzętu na zakończoną budowę.");

            var equipmentIds = request.Items
                .Select(x => x.EquipmentId)
                .ToList();

            var siteEquipments = await _context.SiteEquipments
                .Include(x => x.Equipment)
                .Where(x =>
                    x.SiteId == request.FromConstructionSiteId &&
                    equipmentIds.Contains(x.EquipmentId))
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                var siteEquipment = siteEquipments
                    .FirstOrDefault(
                        x => x.EquipmentId == item.EquipmentId);

                if (siteEquipment is null)
                {
                    throw new InvalidOperationException(
                        "Sprzęt nie znajduje się na budowie źródłowej.");
                }



                if (siteEquipment.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Brak wystarczającej ilości sprzętu: {siteEquipment.Equipment.Name}");
                }

                // Pobieramy aktywne alokacje tego sprzętu
                // na budowie źródłowej.
                var allocations = await _context.SiteEquipmentAllocations
                    .Where(x =>
                        x.SiteId == request.FromConstructionSiteId &&
                        x.EquipmentId == item.EquipmentId &&
                        x.DateTo == null)
                    .OrderBy(x => x.DateFrom)
                    .ToListAsync(cancellationToken);

                var allocatedQuantity = allocations.Sum(x => x.Quantity);
                if (allocatedQuantity > siteEquipment.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Historia przydziału przekracza stan sprzętu na budowie: {siteEquipment.Equipment.Name}");
                }

                // Stan początkowy może nie mieć historii alokacji.
                var quantityWithoutAllocation = siteEquipment.Quantity - allocatedQuantity;

                var quantityToClose = item.Quantity;
                var now = DateTime.UtcNow;

                foreach (var allocation in allocations)
                {
                    if (quantityToClose == 0)
                        break;

                    // Cała alokacja opuszcza budowę
                    if (quantityToClose >= allocation.Quantity)
                    {
                        allocation.DateTo = now;

                        quantityToClose -= allocation.Quantity;
                    }
                    // Tylko część alokacji opuszcza budowę
                    else
                    {
                        var closedAllocation =
                            new SiteEquipmentAllocation
                            {
                                SiteId = allocation.SiteId,
                                EquipmentId = allocation.EquipmentId,
                                Quantity = quantityToClose,
                                DateFrom = allocation.DateFrom,
                                DateTo = now,
                                DayPrice = allocation.DayPrice
                            };

                        _context.SiteEquipmentAllocations.Add(
                            closedAllocation);

                        // Reszta nadal pozostaje na budowie
                        allocation.Quantity -= quantityToClose;

                        quantityToClose = 0;
                    }
                }

                // Nie tworzymy fikcyjnej historii dla stanu początkowego.
                if (quantityToClose > quantityWithoutAllocation)
                {
                    throw new InvalidOperationException(
                        $"Brak wystarczającej aktywnej alokacji dla sprzętu: {siteEquipment.Equipment.Name}");
                }

                // Sprzęt schodzi z budowy źródłowej
                siteEquipment.Quantity -= item.Quantity;

                if (siteEquipment.Quantity == 0)
                {
                    _context.SiteEquipments.Remove(siteEquipment);
                }

                // Sprzęt jest teraz w transporcie
                siteEquipment.Equipment.QuantityInTransit +=
                    item.Quantity;
            }

            var newTransfer = new EquipmentMovement
            {
                Number = await _numberGenerator.GenerateNumberAsync(
                    EquipmentMovementType.Transfer,
                    cancellationToken),

                Type = EquipmentMovementType.Transfer,
                Status = EquipmentMovementStatus.InTransit,

                FromConstructionSiteId =
                    request.FromConstructionSiteId,

                ToConstructionSiteId =
                    request.ToConstructionSiteId,

                AddedByUserId = userId,

                Items = request.Items.Select(
                    x => new EquipmentMovementItem
                    {
                        EquipmentId = x.EquipmentId,
                        Quantity = x.Quantity,
                        ScrappedQuantity = 0
                    })
                    .ToList()
            };

            _context.EquipmentMovements.Add(newTransfer);

            var notification = new Notification
            {
                Message =
                    $"Nowe przesunięcie {newTransfer.Number} oczekuje na potwierdzenie odbioru.",

                ActionUrl =
                    $"/transfers/{newTransfer.Id}"
            };

            _context.Notifications.Add(notification);

            // Powiadomienie dla użytkowników przypisanych
            // do budowy docelowej
            var targetUserIds = await _context.SiteUsers
                .Where(x =>
                    x.SiteId == request.ToConstructionSiteId &&
                    x.UserId != userId &&
                    _context.UserRoles.Any(ur => ur.UserId == x.UserId &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Kierownik Budowy")))
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            foreach (var targetUserId in targetUserIds)
            {
                var recipient = new NotificationRecipient
                {
                    Notification = notification,
                    UserId = targetUserId,
                    IsRead = false
                };

                _context.Recipients.Add(recipient);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (targetUserIds.Count > 0)
            {
                await _hubContext.Clients.Users(targetUserIds)
                    .SendAsync(
                        "ReceiveNotification",
                        new NotificationDto
                        {
                            Id = notification.Id,
                            Message = notification.Message,
                            ActionUrl = notification.ActionUrl,
                            IsRead = false,
                            DateOfNotification =
                                notification.CreatedAt
                        },
                        cancellationToken);
            }

            return newTransfer.Id;
        }
    }
}
