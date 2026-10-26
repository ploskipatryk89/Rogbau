using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Models
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
        public Guid SiteId { get; set; } 
        public Site Site { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        

    }
}
