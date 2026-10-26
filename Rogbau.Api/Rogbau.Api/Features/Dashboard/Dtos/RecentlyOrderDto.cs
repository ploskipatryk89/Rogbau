using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Dashboard.Dtos
{
    public record RecentlyOrderDto
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public string SiteName { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }

    }
}
