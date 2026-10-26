using MediatR;
using Rogbau.Api.Features.Equipments.Dtos;

namespace Rogbau.Api.Features.Equipments.Queries.GetEquipmentById
{
    public record GetEquipmentByIdQuery(Guid Id) : IRequest<EquipmentDetailsDto>;
}
