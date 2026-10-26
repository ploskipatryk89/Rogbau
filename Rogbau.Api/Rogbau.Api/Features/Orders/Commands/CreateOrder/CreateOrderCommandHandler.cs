using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Rogbau.Api.Features.Notifications.Dtos;

namespace Rogbau.Api.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;
        private readonly UserManager<ApplicationUser> _users;
        private readonly IHubContext<NotificationHub> _hub;

        public CreateOrderCommandHandler(ApplicationDbContext context, ICurrentUserService user,
            UserManager<ApplicationUser> users, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _user = user;
            _users = users;
            _hub = hub;
        }

        public async Task<Guid> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_user.UserId))
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany.");
            if (!_user.IsInRole("Kierownik Budowy"))
                throw new UnauthorizedAccessException("Zamówienia sprzętu może tworzyć tylko kierownik budowy.");
            if (!await _context.SiteUsers.AnyAsync(
                x => x.SiteId == request.SiteId && x.UserId == _user.UserId, cancellationToken))
                throw new UnauthorizedAccessException("Możesz zamawiać sprzęt tylko dla swojej budowy.");

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var site = await _context.Sites
                .FirstOrDefaultAsync(
                    s => s.Id == request.SiteId,
                    cancellationToken);

            if (site == null)
            {
                throw new KeyNotFoundException("Nie znaleziono budowy");
            }

            if (site.Status == SiteStatus.Completed)
                throw new InvalidOperationException("Nie można zamawiać sprzętu dla zakończonej budowy.");



            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {


                var equipment = await _context.Equipments
                    .FirstOrDefaultAsync(
                        e => e.Id == item.EquipmentId,
                        cancellationToken);

                if (equipment == null)
                {
                    throw new KeyNotFoundException(
                        $"Nie znaleziono sprzętu o ID {item.EquipmentId}");
                }

                orderItems.Add(new OrderItem
                {
                    EquipmentId = item.EquipmentId,
                    Quantity = item.Quantity
                });
            }

            var orderNumber = await GenerateOrderNumberAsync(
                cancellationToken);

            var order = new Order
            {
                SiteId = request.SiteId,
                Note = request.Note?.Trim(),
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending,
                Items = orderItems
            };

            _context.Orders.Add(order);

            var recipients = (await _users.GetUsersInRoleAsync("Admin"))
                .Concat(await _users.GetUsersInRoleAsync("Magazynier"))
                .Select(x => x.Id).Distinct().ToList();
            var notification = new Notification
            {
                Message = $"Nowe zamówienie {order.OrderNumber} z budowy {site.Name}.",
                ActionUrl = $"/orders/{order.Id}"
            };
            if (recipients.Count > 0)
            {
                _context.Notifications.Add(notification);
                foreach (var recipient in recipients)
                    _context.Recipients.Add(new NotificationRecipient
                    {
                        Notification = notification, UserId = recipient, IsRead = false
                    });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (recipients.Count > 0)
                await _hub.Clients.Users(recipients).SendAsync("ReceiveNotification", new NotificationDto
                {
                    Id = notification.Id, Message = notification.Message,
                    ActionUrl = notification.ActionUrl, IsRead = false,
                    DateOfNotification = notification.CreatedAt
                }, cancellationToken);

            return order.Id;
        }

        private async Task<string> GenerateOrderNumberAsync(
            CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"ZAM/{year}/";

            var lastOrderNumber = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .Select(o => o.OrderNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;

            if (lastOrderNumber != null)
            {
                var numberPart = lastOrderNumber
                    .Substring(prefix.Length);

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }
    }
}
