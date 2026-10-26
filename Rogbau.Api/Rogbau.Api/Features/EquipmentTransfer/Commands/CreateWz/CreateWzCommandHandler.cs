using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Features.Notifications.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreateWz
{
    public class CreateWzCommandHandler
        : IRequestHandler<CreateWzCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _userService;
        private readonly NumberGenerator _numberGenerator;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateWzCommandHandler(
            ApplicationDbContext context,
            ICurrentUserService userService,
            NumberGenerator numberGenerator,
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userService = userService;
            _numberGenerator = numberGenerator;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task<Guid> Handle(
            CreateWzCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _userService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "Użytkownik nie jest zalogowany.");
            }


            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var targetSite = await _context.Sites.FirstOrDefaultAsync(s => s.Id == request.ToConstructionSiteId, cancellationToken);
            if (targetSite == null)
                throw new KeyNotFoundException("Nie znaleziono budowy docelowej.");
            if (targetSite.Status == SiteStatus.Completed)
                throw new InvalidOperationException("Nie można wydać sprzętu na zakończoną budowę.");

            Order? order = null;
            var sentQuantities = new Dictionary<Guid, int>();
            if (request.OrderId.HasValue)
            {
                order = await _context.Orders.Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.OrderId.Value, cancellationToken)
                    ?? throw new KeyNotFoundException("Nie znaleziono zamówienia.");
                if (order.SiteId != request.ToConstructionSiteId || order.Status == OrderStatus.Cancelled)
                    throw new InvalidOperationException("Zamówienie nie dotyczy tej budowy lub zostało anulowane.");

                sentQuantities = await _context.EquipmentMovementItems
                    .Where(x => x.EquipmentMovement.OrderId == order.Id &&
                        x.EquipmentMovement.Status != EquipmentMovementStatus.Cancelled)
                    .GroupBy(x => x.EquipmentId)
                    .Select(x => new { Id = x.Key, Quantity = x.Sum(i => i.Quantity) })
                    .ToDictionaryAsync(x => x.Id, x => x.Quantity, cancellationToken);

                foreach (var item in request.Items)
                {
                    var orderedItem = order.Items.FirstOrDefault(x => x.EquipmentId == item.EquipmentId);
                    if (orderedItem == null || item.Quantity > orderedItem.Quantity - sentQuantities.GetValueOrDefault(item.EquipmentId))
                        throw new InvalidOperationException("Ilość wysyłana przekracza pozostałą ilość zamówienia.");
                }
            }

            var equipmentIds = request.Items
                .Select(x => x.EquipmentId)
                .ToList();

            var equipments = await _context.Equipments
                .Where(x => equipmentIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                var equipment = equipments
                    .FirstOrDefault(x => x.Id == item.EquipmentId);

                if (equipment is null)
                {
                    throw new KeyNotFoundException(
                        "Nie znaleziono sprzętu.");
                }

                if (!equipment.IsActive)
                    throw new InvalidOperationException($"Nie można wydać nieaktywnego sprzętu: {equipment.Name}");

                if (equipment.QuantityAvailable < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Brak wystarczającej ilości sprzętu: {equipment.Name}");
                }

                equipment.QuantityAvailable -= item.Quantity;
                equipment.QuantityInTransit += item.Quantity;
            }

            var newWz = new EquipmentMovement
            {
                Note = request.Note?.Trim(),
                OrderId = request.OrderId,
                Number = await _numberGenerator.GenerateNumberAsync(
                    EquipmentMovementType.WZ,
                    cancellationToken),

                Type = EquipmentMovementType.WZ,

                Status =
                    EquipmentMovementStatus.InTransit,

                ToConstructionSiteId =
                    request.ToConstructionSiteId,

                AddedByUserId =
                    userId,

                Items = request.Items
                    .Select(x => new EquipmentMovementItem
                    {
                        EquipmentId = x.EquipmentId,
                        Quantity = x.Quantity,
                        ScrappedQuantity = 0
                    })
                    .ToList()
            };

            _context.EquipmentMovements.Add(newWz);

            if (order != null)
            {
                foreach (var item in request.Items)
                    sentQuantities[item.EquipmentId] = sentQuantities.GetValueOrDefault(item.EquipmentId) + item.Quantity;
                order.Status = order.Items.All(x => sentQuantities.GetValueOrDefault(x.EquipmentId) >= x.Quantity)
                    ? OrderStatus.Completed : OrderStatus.PartiallySent;
            }

            /*
             * Najpierw pobieramy użytkowników przypisanych
             * do budowy docelowej.
             */
            var assignedUsers = await _context.SiteUsers
                .AsNoTracking()
                .Where(x =>
                    x.SiteId == request.ToConstructionSiteId &&
                    x.UserId != userId)
                .Select(x => x.User)
                .ToListAsync(cancellationToken);

            /*
             * Z przypisanych użytkowników wybieramy WYŁĄCZNIE
             * osoby posiadające rolę "Kierownik Budowy".
             */
            var targetUserIds = new List<string>();

            foreach (var assignedUser in assignedUsers)
            {
                if (await _userManager.IsInRoleAsync(
                        assignedUser,
                        "Kierownik Budowy"))
                {
                    targetUserIds.Add(assignedUser.Id);
                }
            }

            targetUserIds = targetUserIds
                .Distinct()
                .ToList();

            /*
             * Powiadomienie tworzymy tylko wtedy,
             * kiedy istnieje kierownik, który ma je dostać.
             */
            Notification? notification = null;

            if (targetUserIds.Count > 0)
            {
                notification = new Notification
                {
                    Message =
                        $"Nowe wydanie WZ {newWz.Number} oczekuje na potwierdzenie odbioru",

                    ActionUrl =
                        $"/transfers/{newWz.Id}"
                };

                _context.Notifications.Add(notification);

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
            }

            /*
             * Zapisujemy:
             * - WZ,
             * - zmianę stanów,
             * - notification,
             * - recipients.
             */
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            /*
             * Dopiero po zapisie wysyłamy SignalR.
             */
            if (notification is not null &&
                targetUserIds.Count > 0)
            {
                await _hubContext.Clients
                    .Users(targetUserIds)
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

            return newWz.Id;
        }
    }
}
