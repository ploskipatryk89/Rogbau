using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Features.Notifications.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreatePz
{
    public class CreatePzCommandHandler : IRequestHandler<CreatePzCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _userService;
        private readonly NumberGenerator _numberGenerator;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CreatePzCommandHandler(
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
            CreatePzCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _userService.UserId;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany.");
            if (!_userService.IsInRole("Kierownik Budowy") ||
                    !await _context.SiteUsers.AnyAsync(x => x.SiteId == request.FromConstructionSiteId &&
                        x.UserId == userId, cancellationToken))
                    throw new UnauthorizedAccessException("Możesz zgłosić zwrot tylko ze swojej budowy.");

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var equipmentIds = request.Items
                .Select(x => x.EquipmentId)
                .ToList();
            var departureAt = DateTime.UtcNow;

            var equipments = await _context.Equipments
                .Where(x => equipmentIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var siteEquipments = await _context.SiteEquipments
                .Where(x =>
                    x.SiteId == request.FromConstructionSiteId &&
                    equipmentIds.Contains(x.EquipmentId))
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                var equipment = equipments
                    .FirstOrDefault(x => x.Id == item.EquipmentId);

                if (equipment is null)
                    throw new InvalidOperationException("Nie znaleziono sprzętu.");

                var siteEquipment = siteEquipments
                    .FirstOrDefault(x => x.EquipmentId == item.EquipmentId);

                if (siteEquipment is null)
                {
                    throw new InvalidOperationException(
                        $"Sprzęt {equipment.Name} nie znajduje się na tej budowie.");
                }



                if (siteEquipment.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Budowa nie posiada wystarczającej ilości sprzętu: {equipment.Name}");
                }



                // Pobieramy aktywne alokacje sprzętu na budowie.
                // Najstarsze zamykamy jako pierwsze.
                var allocations = await _context.SiteEquipmentAllocations
                    .Where(x =>
                        x.SiteId == request.FromConstructionSiteId &&
                        x.EquipmentId == item.EquipmentId &&
                        x.DateTo == null)
                    .OrderBy(x => x.DateFrom)
                    .ToListAsync(cancellationToken);

                var allocatedQuantity = allocations.Sum(x => x.Quantity);
                if (allocatedQuantity > siteEquipment.Quantity)
                    throw new InvalidOperationException(
                        $"Historia przydziału przekracza stan sprzętu na budowie: {equipment.Name}");

                // Stan początkowy może nie mieć historii przydziału.
                var quantityWithoutAllocation = siteEquipment.Quantity - allocatedQuantity;
                var quantityToClose = item.Quantity;
                var now = departureAt;

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
                        var closedAllocation = new SiteEquipmentAllocation
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

                        // W aktywnej alokacji zostaje tylko sprzęt,
                        // który nadal znajduje się na budowie
                        allocation.Quantity -= quantityToClose;

                        quantityToClose = 0;
                    }
                }

                // Brak historii stanu początkowego nie blokuje faktycznego zwrotu.
                if (quantityToClose > quantityWithoutAllocation)
                {
                    throw new InvalidOperationException(
                        $"Brak wystarczającej aktywnej alokacji dla sprzętu: {equipment.Name}");
                }

                // Całość PZ schodzi z budowy
                siteEquipment.Quantity -= item.Quantity;

                if (siteEquipment.Quantity == 0)
                {
                    _context.SiteEquipments.Remove(siteEquipment);
                }

                // Magazyn przyjmie sprzęt dopiero po potwierdzeniu odbioru.
                equipment.QuantityInTransit += item.Quantity;
            }

            var newPz = new EquipmentMovement
            {
                Note = request.Note?.Trim(),
                CreatedAt = departureAt,
                Number = await _numberGenerator.GenerateNumberAsync(
                    EquipmentMovementType.PZ,
                    cancellationToken),

                Type = EquipmentMovementType.PZ,
                Status = EquipmentMovementStatus.InTransit,

                FromConstructionSiteId =
                    request.FromConstructionSiteId,

                ToConstructionSiteId = null,

                AddedByUserId = userId,

                Items = request.Items.Select(x =>
                    new EquipmentMovementItem
                    {
                        EquipmentId = x.EquipmentId,
                        Quantity = x.Quantity,
                        ScrappedQuantity = x.QuantityScrapped
                    })
                    .ToList()
            };

            _context.EquipmentMovements.Add(newPz);

            var recipients = await (from userRole in _context.UserRoles
                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                    where role.Name == "Admin" || role.Name == "Magazynier"
                                    select userRole.UserId).Distinct().ToListAsync(cancellationToken);
            var notification = new Notification
            {
                Message = $"Zwrot PZ {newPz.Number} oczekuje na odbiór w magazynie.",
                ActionUrl = $"/transfers/{newPz.Id}"
            };
            if (recipients.Count > 0)
            {
                _context.Notifications.Add(notification);
                foreach (var recipient in recipients)
                    _context.Recipients.Add(new NotificationRecipient
                    { Notification = notification, UserId = recipient, IsRead = false });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (recipients.Count > 0)
                await _hubContext.Clients.Users(recipients).SendAsync("ReceiveNotification", new NotificationDto
                {
                    Id = notification.Id, Message = notification.Message, ActionUrl = notification.ActionUrl,
                    IsRead = false, DateOfNotification = notification.CreatedAt
                }, cancellationToken);

            return newPz.Id;
        }
    }
}
