namespace Rogbau.Api.Features.Purchases.Commands.EditPurchase
{
    public record EditPurchaseCommand : MediatR.IRequest<MediatR.Unit>
    {
        public Guid Id { get; init; }
        public Guid SupplierId { get; init; }
        public DateOnly PurchaseDate { get; init; }
        public List<Rogbau.Api.Features.Purchases.Dtos.CreatePurchaseItemDto> Items { get; init; } = [];
    }
}
