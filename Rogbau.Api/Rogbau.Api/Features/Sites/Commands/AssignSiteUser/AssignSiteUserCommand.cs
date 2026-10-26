using MediatR;

namespace Rogbau.Api.Features.Sites.Commands.AssignSiteUser
{
    public record AssignSiteUserCommand(
      Guid SiteId,
      string UserId
  ) : IRequest;
}
