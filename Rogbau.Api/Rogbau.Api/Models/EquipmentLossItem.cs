using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Models
{
    public class EquipmentLossItem : BaseEntity
    {
        public Guid EquipmentLossId { get; set; }
        public EquipmentLoss EquipmentLoss { get; set; }
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        public EquipmentLossType LossType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

    }
}
