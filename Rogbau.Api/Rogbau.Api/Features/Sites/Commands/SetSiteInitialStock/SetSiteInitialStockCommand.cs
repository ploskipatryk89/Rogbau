using MediatR;
using Rogbau.Api.Features.Sites.Dtos;

namespace Rogbau.Api.Features.Sites.Commands.SetSiteInitialStock
{
    public record SetSiteInitialStockCommand : IRequest
    {
        public Guid SiteId { get; set; }

        public List<SetSiteInitialStockItemDto> Items { get; init; } = [];
    }
}
