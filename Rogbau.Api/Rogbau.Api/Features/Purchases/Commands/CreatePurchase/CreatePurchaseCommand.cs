using MediatR;
using Rogbau.Api.Features.Purchases.Dtos;

namespace Rogbau.Api.Features.Purchases.Commands;

public record CreatePurchaseCommand : IRequest<Guid>
{
    public Guid SupplierId { get; init; }
    public DateOnly PurchaseDate { get; init; }
    public List<CreatePurchaseItemDto> Items { get; init; } = [];
}