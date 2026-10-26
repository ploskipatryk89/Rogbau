namespace Rogbau.Api.Features.HistoricalData.Dtos
{
    public class HistoricalDataDto
    {
        public Guid Id { get; init; }

        public Guid SiteId { get; init; }
        public string SiteName { get; init; } = string.Empty;

        public Guid EquipmentId { get; init; }
        public string EquipmentName { get; init; } = string.Empty;

        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }

        public int Quantity { get; init; }

        public decimal DayPrice { get; init; }
        public int Days { get; init; }

        public decimal TotalEarned { get; init; }
    }
}
