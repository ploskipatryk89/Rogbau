namespace Rogbau.Api.Features.Dashboard.Dtos
{
    public class AdminDashboardDto
    {
        public string DashboardType => "admin";
        public int ActiveSiteQuantity { get; init; }
        public int EquipmentAvailable { get; init; }
        public int EquipmentsOnAllSites { get; init; }

        public List<RecentlyDocumentDto> RecentlyDocuments { get; init; } = new List<RecentlyDocumentDto>();
        public List<RecentlyOrderDto> RecentlyOrders { get; init; } = new List<RecentlyOrderDto>();
        public List<LowEquipmentStockDto> LowEquipmentStocks { get; init; } = new List<LowEquipmentStockDto>();

    }
}
