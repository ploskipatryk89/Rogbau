using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Rogbau.Api.Features.Notifications.Dtos;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Notifications.Queries
{
    public class GetMyNotificationQueryHandler : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;

        public GetMyNotificationQueryHandler(ICurrentUserService currentUserService, ApplicationDbContext context)
        {
            _currentUserService = currentUserService;
            _context = context;
        }

        public Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Uzytkownik nie jest zalogowany");
            }

            var query = _context.Recipients
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new NotificationDto
                {
                    Id = r.Notification.Id,
                    Message = r.Notification.Message,
                    ActionUrl = r.Notification.ActionUrl,
                    IsRead = r.IsRead,
                    DateOfNotification = r.Notification.CreatedAt
                    

                })
                .Take(20)
                .ToListAsync(cancellationToken);

            return query;
        }
    }
}
