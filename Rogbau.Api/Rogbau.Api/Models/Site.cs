using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Models
{
    public class Site : BaseEntity
    {
        public string Name { get; set; }
        public string? Address { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public SiteStatus Status { get; set; }

        public SiteStatus GetCurrentStatus(DateOnly today)
        {
            if (Status == SiteStatus.Completed) return SiteStatus.Completed;
            if (StartDate.HasValue)
                return StartDate.Value <= today ? SiteStatus.InProgress : SiteStatus.Planned;
            return Status;
        }

        public List<SiteEquipment> EquipmentItems { get; set; } = new List<SiteEquipment>();
        public ICollection<SiteUser> Users { get; set; } = new List<SiteUser>();
    }
}
