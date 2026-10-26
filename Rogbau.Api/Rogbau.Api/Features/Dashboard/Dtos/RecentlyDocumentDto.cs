using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Dashboard.Dtos
{
    public class RecentlyDocumentDto
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;

        public string From { get; init; } = string.Empty;
        public string To { get; init; } = string.Empty;
        public EquipmentMovementStatus Status { get; init; }
        public DateOnly Date { get; init; }

    }
}
