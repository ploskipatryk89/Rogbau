using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Models
{
    public class EquipmentMovement : BaseEntity
    {

        public string? Note { get; set; }
        public string Number { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public EquipmentMovementType Type { get; set; }

        public EquipmentMovementStatus Status { get; set; }

        public Guid? FromConstructionSiteId { get; set; }
        public Site? FromConstructionSite { get; set; }

        public Guid? ToConstructionSiteId { get; set; }
        public Site? ToConstructionSite { get; set; }

        public string AddedByUserId { get; set; } = string.Empty;
       public ApplicationUser AddedBy { get; set; }

        public List<EquipmentMovementItem> Items { get; set; } = new List<EquipmentMovementItem>();
    }
}
