using MediatR;

namespace Rogbau.Api.Features.Equipments.Commands;

public record CreateEquipmentCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal DayPrice { get; init; }
}
