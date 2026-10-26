using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions.Equipments;
using Rogbau.Api.Features.Notifications.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Equipments.Commands.CreateEquipment
{
    public class CreateEquipmentCommandHandler : IRequestHandler<CreateEquipmentCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ICurrentUserService _userService;

        public CreateEquipmentCommandHandler(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, ICurrentUserService userService)
        {
            _context = context;
            _hubContext = hubContext;
            _userService = userService;
        }

        public async Task<Guid> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _userService.UserId;
          

            var newEquipment = new Equipment
            {
                Name = request.Name.Trim(),
                Unit = request.Unit,
                WeightKg = request.WeightKg,
                CategoryId = request.CategoryId,
                SubCategoryId = request.SubCategoryId,
                InitialQuantity = 0,
                DayPrice = request.DayPrice,
                PurchasePrice = request.PurchasePrice,

                QuantityAvailable = 0,

                PurchasedQuantity = 0,
                QuantityScrapped = 0,

                IsActive = true
            };

            _context.Equipments.Add(newEquipment);

            // Najpierw zapisujemy sprzęt, żeby baza nadała mu ID i uniknąć pustego Guida w URL
            await _context.SaveChangesAsync(cancellationToken);

            var notification = new Notification
            {
                Message = $"Dodano nowy sprzęt do bazy: {newEquipment.Name}",
                ActionUrl = $"/equipments/{newEquipment.Id}",
                
            };

            _context.Notifications.Add(notification);

            var targetUserIds = await _context.Users
    .Where(u => u.Id != currentUserId)
    .Select(u => u.Id)
    .ToListAsync(cancellationToken);

            foreach (var userId in targetUserIds)
            {
                var recipient = new NotificationRecipient
                {
                    Notification = notification,
                    UserId = userId,
                    IsRead = false
                };

                _context.Recipients.Add(recipient);
            }

            await _context.SaveChangesAsync(cancellationToken);


            await _hubContext.Clients.Users(targetUserIds)
            .SendAsync("ReceiveNotification", new NotificationDto
            {
                Id = notification.Id,
                Message = notification.Message,
                ActionUrl = notification.ActionUrl,
                IsRead = false,
                DateOfNotification = notification.CreatedAt
            }, cancellationToken);

            return newEquipment.Id;
        }
    }
}
