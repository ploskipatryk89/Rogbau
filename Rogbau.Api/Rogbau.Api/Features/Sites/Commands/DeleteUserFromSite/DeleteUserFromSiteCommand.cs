using MediatR;

namespace Rogbau.Api.Features.Sites.Commands.DeleteUserFromSite
{
    public record DeleteUserFromSiteCommand(string UserId, Guid SiteId) : IRequest<Unit>;
    
    
}
