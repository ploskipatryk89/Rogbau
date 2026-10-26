using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _userService;

        public MarkAsReadCommandHandler(ApplicationDbContext context, ICurrentUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Unit> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _userService.UserId;

            var unreadRecipients = await _context.Recipients
                .Where(r => r.UserId == userId && r.NotificationId == request.NotificationId)
                .ToListAsync(cancellationToken);

            if (unreadRecipients.Count == 0)
                throw new KeyNotFoundException("Nie znaleziono powiadomienia.");

            if (unreadRecipients.Any())
            {
                foreach(var recipient in unreadRecipients)
                {
                    recipient.IsRead = true;
                    recipient.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
