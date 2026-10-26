using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Dashboard.Dtos
{
    public class SiteManagerDashboardDto
    {
        public string DashboardType => "siteManager";
        public List<ManagerSiteDto> ManagerSites { get; init; } = new List<ManagerSiteDto>();
        public Guid? SelectedSiteId { get; init; }
        public SiteStatus? Status { get; init; }
        public int SiteEquipmentQuantity { get; init; }
        public int WzQuantityThisMonth { get; init; }
        public int PzQuantityThisMonth { get; init; }
        public List<RecentlyDocumentDto> RecentlyDocuments { get; init; } = new List<RecentlyDocumentDto>();


    }
}
