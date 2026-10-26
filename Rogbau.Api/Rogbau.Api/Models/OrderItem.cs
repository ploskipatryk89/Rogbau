
namespace Rogbau.Api.Models
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        public int Quantity { get; set; }
    }
}
