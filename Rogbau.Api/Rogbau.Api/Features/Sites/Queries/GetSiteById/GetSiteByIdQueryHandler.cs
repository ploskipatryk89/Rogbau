using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Sites.Dtos;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Queries.GetSiteById
{
    public class GetSiteByIdQueryHandler
        : IRequestHandler<GetSiteByIdQuery, SiteDetailsDto?>
    {
        private readonly ApplicationDbContext _context;

        public GetSiteByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SiteDetailsDto?> Handle(
            GetSiteByIdQuery request,
            CancellationToken cancellationToken)
        {
            var site = await _context.Sites
                .AsNoTracking()
                .Include(s => s.EquipmentItems)
                    .ThenInclude(e => e.Equipment)
                .FirstOrDefaultAsync(
                    s => s.Id == request.Id,
                    cancellationToken
                );

            if (site == null)
            {
                return null;
            }

            var equipmentItems = site.EquipmentItems
                .Where(e => e.Quantity > 0)
                .Select(e =>
                {
                    var purchaseTotalPrice =
                        e.Quantity * e.Equipment.PurchasePrice;

                    return new SiteEquipmentDto(
                        e.EquipmentId,
                        e.Equipment.Name,
                        e.Equipment.Unit,
                        e.Quantity,
                        purchaseTotalPrice,
                        e.Equipment.WeightKg
                    );
                })
                .ToList();

            var siteManagers = await _context.SiteUsers
                .AsNoTracking()
                .Where(x => x.SiteId == site.Id)
                .Select(x => new SiteManagerDto
                {
                    Id = x.UserId,
                    FullName = $"{x.User.FirstName} {x.User.LastName}",
                    Email = x.User.Email ?? string.Empty,
                    PhoneNumber = x.User.PhoneNumber ?? string.Empty
                })
                .ToListAsync(cancellationToken);

            return new SiteDetailsDto(
                site.Id,
                site.Name,
                site.Address,
                site.StartDate,
                site.EndDate,
                site.GetCurrentStatus(DateOnly.FromDateTime(DateTime.Today)),
                equipmentItems,
                siteManagers
            );
        }
    }
}
