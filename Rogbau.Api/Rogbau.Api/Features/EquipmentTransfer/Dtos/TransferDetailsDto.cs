using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.EquipmentTransfer.Dtos
{
    public record TransferDetailsDto
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public string? Note { get; set; }
        public string Number { get; set; } = string.Empty;

        public Guid? FromConstructionSiteId { get; set; }
        public string? FromName { get; set; }

        public Guid? ToConstructionSiteId { get; set; }
        public string? ToName { get; set; }

        public DateTime Date { get; set; }

        public EquipmentMovementStatus Status { get; set; }
        public EquipmentMovementType Type { get; set; }

        public List<TransferDetailsItemDto> Items { get; set; } = new();

        public bool CanConfirm { get; set; }
        public decimal TotalWeight { get; set; }
    }

    public record TransferDetailsItemDto
    {
        public Guid EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public int ScrappedQuantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public int? ReceivedScrappedQuantity { get; set; }
    }
}

