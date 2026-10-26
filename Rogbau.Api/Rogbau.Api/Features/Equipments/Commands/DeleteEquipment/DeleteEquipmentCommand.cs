using MediatR;

namespace Rogbau.Api.Features.Equipments.Commands.DeleteEquipment
{
    public record DeleteEquipmentCommand(Guid Id) : IRequest<Unit>;
    
}
