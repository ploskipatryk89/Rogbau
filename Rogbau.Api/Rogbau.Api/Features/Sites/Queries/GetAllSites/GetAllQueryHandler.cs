using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Sites.Dtos;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Queries.GetAllSites
{
    public class GetAllQueryHandler : IRequestHandler<GetAllQuery, List<SiteDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SiteDto>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Sites
                .AsNoTracking()
                .Select(s => new SiteDto(
                    s.Id,
                    s.Name,
                    s.Address,
                    s.StartDate,
                    s.EndDate,
                    s.Status == Rogbau.Api.Models.Enums.SiteStatus.Completed
                        ? s.Status
                        : s.StartDate.HasValue
                            ? (s.StartDate.Value <= today ? Rogbau.Api.Models.Enums.SiteStatus.InProgress : Rogbau.Api.Models.Enums.SiteStatus.Planned)
                            : s.Status,
                    s.EquipmentItems.Sum(e => (int)e.Quantity)
                    
                    
                    // LUB s.EquipmentItems.Count jeśli chcesz zliczać pozycje
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
