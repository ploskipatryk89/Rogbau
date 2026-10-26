using MediatR;

namespace Rogbau.Api.Features.Notifications.Commands.MarkAsRead
{
    public record MarkAsReadCommand : IRequest<Unit>

    {
        public Guid NotificationId { get; init; }
    }
}
