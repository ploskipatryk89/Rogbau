using MediatR;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreateTransfer
{
    public record CreateTransferCommand : IRequest<Guid>
    {
        public Guid FromConstructionSiteId { get; set; }
        public Guid ToConstructionSiteId { get; set; }
        public List<CreateTransferItemDto> Items { get; set; } = new();
    }
}