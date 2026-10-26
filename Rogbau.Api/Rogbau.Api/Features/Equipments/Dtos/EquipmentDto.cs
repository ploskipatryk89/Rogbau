namespace Rogbau.Api.Features.Equipments.Dtos
{
    public record EquipmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public Guid? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }

        public int InitialQuantity { get; set; }
        public int PurchasedQuantity { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityScrapped { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal DayPrice { get; set; }
        public bool IsActive { get; set; }
    }
}