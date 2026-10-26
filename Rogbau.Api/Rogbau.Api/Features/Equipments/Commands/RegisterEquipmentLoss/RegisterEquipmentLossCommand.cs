using MediatR;
using Rogbau.Api.Features.Equipments.Commands.RegisterEquipmentLoss.Enums;

namespace Rogbau.Api.Features.Equipments.Commands.RegisterEquipmentLoss
{
    public record RegisterEquipmentLossCommand(Guid Id, EquipmentLossType Type) : IRequest<Unit>;
    
    
}
