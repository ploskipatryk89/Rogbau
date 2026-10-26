using MediatR;
using Rogbau.Api.Features.Notifications.Dtos;

namespace Rogbau.Api.Features.Notifications.Queries
{
    public record GetMyNotificationsQuery() : IRequest<List<NotificationDto>>;
    
    
}
