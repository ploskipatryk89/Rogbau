using MediatR;
using System.Text.Json.Serialization;

namespace Rogbau.Api.Features.Equipments.Commands.EditEquipment
{
    public record EditEquipmentCommand
    (
        
        Guid Id,
         string Name,
        string Unit,
        decimal WeightKg,
        Guid CategoryId,
        Guid? SubCategoryId,
        
        decimal PurchasePrice,
        decimal DayPrice
    ) : IRequest<Unit>;
    
}
