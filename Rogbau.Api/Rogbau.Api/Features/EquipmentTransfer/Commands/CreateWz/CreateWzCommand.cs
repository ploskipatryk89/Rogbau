using MediatR;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreateWz
{
    public class CreateWzCommand : IRequest<Guid>
    {
        public string? Note { get; init; }
        public Guid ToConstructionSiteId { get; init; }
        public Guid? OrderId { get; init; }

        public List<CreateWzItemDto> Items { get; init; } = [];
    }
}
