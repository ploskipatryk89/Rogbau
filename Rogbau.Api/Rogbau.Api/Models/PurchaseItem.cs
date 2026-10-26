namespace Rogbau.Api.Models
{
    public class PurchaseItem : BaseEntity
    {
        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
       
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        
        public int Quantity { get; set; }

        public decimal PurchasePrice { get; set; }

        
    }
}