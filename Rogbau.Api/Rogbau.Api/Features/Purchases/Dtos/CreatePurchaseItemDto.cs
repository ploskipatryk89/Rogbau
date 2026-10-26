namespace Rogbau.Api.Features.Purchases.Dtos;

public record CreatePurchaseItemDto
{
    public Guid EquipmentId { get; init; }
    public decimal PurchasePrice { get; init; }
    public int Quantity { get; init; }
}