using MediatR;
using Rogbau.Api.Features.Suppliers.Dtos;

namespace Rogbau.Api.Features.Suppliers.Queries.GetAllSuppliers
{
    public record GetAllSuppliersQuery : IRequest<List<SupplierResponseDto>>
    {
    }
}
