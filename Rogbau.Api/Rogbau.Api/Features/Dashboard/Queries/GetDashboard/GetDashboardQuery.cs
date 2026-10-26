using MediatR;

namespace Rogbau.Api.Features.Dashboard.Queries.GetDashboard
{
    public record GetDashboardQuery : IRequest<object>
    {
        public Guid? SiteId { get; init; }
    }
}
