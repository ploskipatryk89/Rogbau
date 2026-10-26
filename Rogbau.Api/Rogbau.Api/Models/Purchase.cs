namespace Rogbau.Api.Models
{
    public class Purchase : BaseEntity
    {
        public string PurchaseNumber { get; set; }
        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        
        public DateOnly PurchaseDate { get; set; }
        public List<PurchaseItem> Items { get; set; }

        public int TotalQuantity { get; set; }

        public decimal TotalPurchasePrice { get; set; }
    }
}
