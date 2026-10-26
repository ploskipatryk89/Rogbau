using MediatR;
using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Sites.Commands.CreateSite
{
    public record CreateSiteCommand
    (
        string Name,
        string? Address,
        DateOnly? StartDate,
        DateOnly? EndDate,
        SiteStatus Status,
        List<string> UserIds
    ) : IRequest<Guid>;
}