namespace Rogbau.Api.Features.Orders.Dtos
{
    public class OrderItemDto
    {
        public Guid EquipmentId { get; init; }
        public int Quantity { get; set; }
    }
}
