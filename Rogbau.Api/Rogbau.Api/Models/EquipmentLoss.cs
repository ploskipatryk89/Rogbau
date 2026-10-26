namespace Rogbau.Api.Models.Enums
{
    public class EquipmentLoss : BaseEntity
    {
 
        public Guid? SiteId { get; set; }
        public Site? Site { get; set; }
        public DateTime LossDate { get; set; }
        public string? Note { get; set; }
        public string ReportedByUserId { get; set; } = string.Empty;
        public ApplicationUser ReportedByUser { get; set; }
        public List<EquipmentLossItem> Items { get; set; } = new();
    }
}
