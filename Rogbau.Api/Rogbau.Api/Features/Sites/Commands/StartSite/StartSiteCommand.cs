using MediatR;

namespace Rogbau.Api.Features.Sites.Commands.StartSite
{
    public record StartSiteCommand : IRequest<Unit>
    {
        public Guid Id { get; init; }
        public DateOnly StartDate { get; init; }
    }
}
