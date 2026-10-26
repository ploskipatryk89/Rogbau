namespace Rogbau.Api.Models
{
    public class SiteEquipment : BaseEntity
    {
        public Guid SiteId { get; set; }
        public Site Site { get; set; } = null!;

        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;

        public int Quantity { get; set; }

    }
}
