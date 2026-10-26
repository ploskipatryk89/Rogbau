using MediatR;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.EquipmentTransfer.Queries.GetAllTransfers
{
    public record GetAllTransfersQuery : IRequest<PaginatedList<TransferDto>>
    {
        public EquipmentMovementType? Type { get; set; }
        public string? Search { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public PaginatedRequest Pagination { get; set; } = new();
    }
}
