using MediatR;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditWz
{
    public class EditWzCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        
        public Guid ToConstructionSiteId { get; set; }
        public Guid? OrderId { get; set; }
        public string? Note { get; set; }
        public List<EditMovementItemDto> Items { get; set; } = new List<EditMovementItemDto>();
    }
}
