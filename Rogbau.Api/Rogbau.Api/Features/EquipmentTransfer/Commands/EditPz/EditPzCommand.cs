using MediatR;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditPz
{
    public class EditPzCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public Guid FromConstructionSiteId { get; set; }
        public string? Note { get; set; }
        public List<EditPzItemDto> Items { get; set; } = new List<EditPzItemDto>();
    }
}
