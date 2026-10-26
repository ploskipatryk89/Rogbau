using MediatR;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Equipments.Dtos;

namespace Rogbau.Api.Features.Equipments.Queries.GetAllEquipments
{
    public record GetAllEquipmentsQuery(
        string? SearchQuery = null,
        bool? IsActive = null,
        Guid? CategoryId = null,
        Guid? SubCategoryId = null
    ) : IRequest<PaginatedList<EquipmentDto>>
    {
        public PaginatedRequest Pagination { get; init; } = new();
    }
}