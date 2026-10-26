using MediatR;

namespace Rogbau.Api.Features.Notifications.Commands.DeleteNotification;

public record DeleteNotificationCommand(Guid NotificationId) : IRequest;
