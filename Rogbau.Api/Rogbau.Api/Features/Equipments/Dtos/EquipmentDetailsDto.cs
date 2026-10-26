namespace Rogbau.Api.Features.Equipments.Dtos;

public record EquipmentDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }

    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;

    public Guid? SubCategoryId { get; init; }
    public string? SubCategoryName { get; init; } = string.Empty;

    public int InitialQuantity { get; init; }
    public int PurchasedQuantity { get; init; }
    public int QuantityAvailable { get; init; }
    public int QuantityScrapped { get; init; }
    public int QuantityOnAllSites { get; init; }
    public int QuantityInTransit { get; set; }
    
    public decimal PurchasePrice { get; init; }
    public decimal DayPrice { get; init; }
    public bool IsActive { get; init; }

    public List<EquipmentSiteAllocationDto> SiteAllocations { get; init; } = new List<EquipmentSiteAllocationDto>();
}