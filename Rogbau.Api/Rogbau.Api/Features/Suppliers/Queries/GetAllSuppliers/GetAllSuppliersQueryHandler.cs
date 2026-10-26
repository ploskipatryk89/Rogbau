using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Suppliers.Dtos;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Suppliers.Queries.GetAllSuppliers
{
    public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, List<SupplierResponseDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllSuppliersQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task <List<SupplierResponseDto>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
        {
            var allSuppliers = await _context.Suppliers
                .AsNoTracking()
                .Select(s => new SupplierResponseDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync(cancellationToken);

            return allSuppliers;
        }
    }
}
