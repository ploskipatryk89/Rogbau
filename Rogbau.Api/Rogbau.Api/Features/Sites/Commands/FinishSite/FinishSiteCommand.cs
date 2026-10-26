using MediatR;

namespace Rogbau.Api.Features.Sites.Commands.FinishSite
{
    public record FinishSiteCommand : IRequest<Unit>
    {
        public Guid Id { get; init; }
        public DateOnly EndDate { get; init; }
    }
}
