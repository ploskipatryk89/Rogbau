using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Orders.Dtos;

public class OrderDetailsDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = "";
    public string? Note { get; set; }
    public DateTime Date { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderDetailsItemDto> Items { get; set; } = [];
    public List<OrderMovementDto> Transfers { get; set; } = [];
}

public class OrderDetailsItemDto
{
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Quantity { get; set; }
    public int SentQuantity { get; set; }
    public int QuantityAvailable { get; set; }
    public decimal WeightKg { get; set; }
}

public class OrderMovementDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public EquipmentMovementStatus Status { get; set; }
}
