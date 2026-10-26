namespace Rogbau.Api.Features.EquipmentTransfer.Dtos
{
    public record CreateWzItemDto
    {
        public Guid EquipmentId { get; init; }
        public int Quantity { get; init; }
    }
}
