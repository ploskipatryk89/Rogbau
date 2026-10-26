using MediatR;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.ConfirmEquipmentMovement
{
    public class ConfirmEquipmentMovementCommand : IRequest
    {
        public Guid Id { get; set; }
        public List<ConfirmReturnItem>? Items { get; set; }
    }

    public class ConfirmReturnItem
    {
        public Guid EquipmentId { get; set; }
        public int Quantity { get; set; }
        public int ScrappedQuantity { get; set; }
    }
}
