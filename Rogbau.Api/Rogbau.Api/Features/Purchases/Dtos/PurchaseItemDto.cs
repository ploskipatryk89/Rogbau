namespace Rogbau.Api.Features.Purchases.Dtos;

public record PurchaseItemDto
{
    public string EquipmentName { get; init; } = string.Empty;
    public Guid EquipmentId { get; init; }
    public string Unit { get; init; } = string.Empty;
    public decimal Quantity { get; init; }


    public decimal PurchaseItemPrice { get; set; }

    public decimal PurchaseItemTotalPrice { get; set; }
}
