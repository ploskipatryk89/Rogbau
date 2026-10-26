using MediatR;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditTransfer
{
    public class EditTransferCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public Guid FromConstructionSiteId { get; set; }
        public Guid ToConstructionSiteId { get; set; }
        
        public string? Note { get; set; }
        public List<EditMovementItemDto> Items { get; set; } = new List<EditMovementItemDto>();
    }
}
