using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.EquipmentTransfer.Dtos
{
    public record TransferDto
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public string? FromName { get; init; } = string.Empty;
        public string? ToName { get; init; } = string.Empty;
        public DateTime Date { get; init; }
        public EquipmentMovementStatus Status { get; init; }
        public EquipmentMovementType Type { get; init; }
        public int QuantityEquipment { get; init; }
    }
}
