namespace Rogbau.Api.Features.Dashboard.Dtos
{
    public class WarehousemanDashboardDto
    {
        public string DashboardType => "warehouseman";
        public int EquipmentAvailable { get; init; }
        public int WzQuantityThisMonth { get; init; }
        public int PzQuantityThisMonth { get; init; }
        public List<RecentlyDocumentDto> RecentlyDocuments { get; init; } = new List<RecentlyDocumentDto>();
        public List<RecentlyOrderDto> RecentlyOrders { get; init; } = new List<RecentlyOrderDto>();
        public List<LowEquipmentStockDto> LowEquipmentStocks { get; init; } = new List<LowEquipmentStockDto>();
    }
}
