using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Notifications.Commands.MarkAsRead;
using Rogbau.Api.Features.Notifications.Dtos;
using Rogbau.Api.Features.Notifications.Queries;

namespace Rogbau.Api.Features.Notifications
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<NotificationDto>>> GetMyNotifications()
        {
            var result = await _mediator.Send(new GetMyNotificationsQuery());
            return Ok(result);
        }

        [HttpPatch("{id:guid}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkAsReadCommand { NotificationId = id }, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Commands.DeleteNotification.DeleteNotificationCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
