namespace Rogbau.Api.Features.Equipments.Dtos
{
    public class EquipmentSiteAllocationDto
    {
        public Guid SiteId { get; set; }
        public string SiteName { get; init; } = string.Empty;
        public string? Address { get; init; } = string.Empty;
        public int QuantityOnSite { get; init; }

    }
}
