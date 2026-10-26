namespace Rogbau.Api.Models
{
    public class EquipmentMovementItem : BaseEntity
    {
        public Guid EquipmentMovementId { get; set; }
        public EquipmentMovement EquipmentMovement { get; set; } = null!;

        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;

        public int Quantity { get; set; }

        public int ScrappedQuantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public int? ReceivedScrappedQuantity { get; set; }
    }
}
