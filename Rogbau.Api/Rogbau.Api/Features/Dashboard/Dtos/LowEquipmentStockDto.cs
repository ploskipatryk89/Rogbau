namespace Rogbau.Api.Features.Dashboard.Dtos
{
    public class LowEquipmentStockDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }
}