using MediatR;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Purchases.Dtos;

namespace Rogbau.Api.Features.Purchases.Queries
{
    public record GetAllPurchasesQuery
    (
    ) : IRequest<PaginatedList<PurchaseListDto>>
    {
        public PaginatedRequest Pagination { get; set; } = new();
    }
    
}
