using MediatR;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreatePz
{
    public class CreatePzCommand : IRequest<Guid>
    {
        public string? Note { get; init; }
        public Guid FromConstructionSiteId { get; init; }

        public List<CreatePzItemDto> Items { get; init; } = [];
    }
}
