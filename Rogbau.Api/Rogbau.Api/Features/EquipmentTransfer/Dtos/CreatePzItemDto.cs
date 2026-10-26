namespace Rogbau.Api.Features.EquipmentTransfer.Dtos
{
    public record CreatePzItemDto
    {
        public Guid EquipmentId { get; init; }
        public int Quantity { get; init; }
        public int QuantityScrapped { get; set; }
    }
}
