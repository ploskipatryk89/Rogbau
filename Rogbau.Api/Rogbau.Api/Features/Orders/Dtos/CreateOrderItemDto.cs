namespace Rogbau.Api.Features.Orders.Dtos
{
    public class CreateOrderItemDto
    {
        public Guid EquipmentId { get; init; }
        public int Quantity { get; set; }
    }
}
