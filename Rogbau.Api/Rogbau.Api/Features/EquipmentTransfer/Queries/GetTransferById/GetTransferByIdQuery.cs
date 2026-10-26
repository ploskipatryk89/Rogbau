using MediatR;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;

namespace Rogbau.Api.Features.EquipmentTransfer.Queries.GetTransferById
{
    public record GetTransferByIdQuery(Guid Id)
        : IRequest<TransferDetailsDto>;
}