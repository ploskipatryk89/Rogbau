using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteNotificationCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany.");
        }

        // Sprawdzenie odbiorców i usunięcie zapisujemy jako jedną operację.
        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var notification = await _context.Notifications
            .Include(n => n.Recipients)
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException("Nie znaleziono powiadomienia.");
        }

        var recipients = notification.Recipients.Where(r => r.UserId == userId).ToList();
        if (recipients.Count == 0)
        {
            throw new KeyNotFoundException("Nie znaleziono powiadomienia.");
        }

        bool hasOtherRecipients = notification.Recipients.Any(r => r.UserId != userId);
        _context.Recipients.RemoveRange(recipients);

        // Wspólna treść znika dopiero po usunięciu jej przez ostatniego odbiorcę.
        if (!hasOtherRecipients)
        {
            _context.Notifications.Remove(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
