using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Rogbau.Api.Models
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "SignalR CONNECTED | ConnectionId: {ConnectionId} | UserIdentifier: {UserIdentifier} | Authenticated: {Authenticated}",
                Context.ConnectionId,
                Context.UserIdentifier,
                Context.User?.Identity?.IsAuthenticated);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            _logger.LogInformation(
                "SignalR DISCONNECTED | ConnectionId: {ConnectionId} | UserIdentifier: {UserIdentifier} | Error: {Error}",
                Context.ConnectionId,
                Context.UserIdentifier,
                exception?.Message);

            await base.OnDisconnectedAsync(exception);
        }
    }
}