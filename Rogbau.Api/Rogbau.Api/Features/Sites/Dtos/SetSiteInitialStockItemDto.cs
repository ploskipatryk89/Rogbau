namespace Rogbau.Api.Features.Sites.Dtos
{
    public record SetSiteInitialStockItemDto
    {
        public Guid EquipmentId { get; init; }
        public int Quantity { get; init; }

    }
}
