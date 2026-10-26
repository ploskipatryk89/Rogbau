namespace Rogbau.Api.Models
{
    public class SiteEquipmentAllocation : BaseEntity
    {
        public Guid SiteId { get; set; }
        public Site Site { get; set; } = null!;

        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;

        public int Quantity { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public decimal DayPrice { get; set; }
    }
}
