using MediatR;
using Rogbau.Api.Features.Orders.Dtos;

namespace Rogbau.Api.Features.Orders.Commands.CreateOrder
{
    public record CreateOrderCommand : IRequest<Guid>
    {
        public Guid SiteId { get; init; }
        public string? Note { get; init; }
        public List<CreateOrderItemDto> Items { get; init; } = new();
    }
}
