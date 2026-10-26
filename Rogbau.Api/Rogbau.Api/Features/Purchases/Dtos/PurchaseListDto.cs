namespace Rogbau.Api.Features.Purchases.Dtos;

public record PurchaseListDto
{
    public Guid Id { get; init; }
    public Guid SupplierId { get; init; }
    public DateOnly Date { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public int TotalItemsCount { get; init; }
    public int TotalQuantity { get; init; }
    public decimal TotalPrice { get; set; }
    public List<PurchaseItemDto> Items { get; init; } = [];
}
