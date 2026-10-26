using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Equipments.Dtos;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Equipments.Queries.GetEquipmentById
{
    public class GetEquipmentByIdQueryHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDetailsDto>
    {
        private readonly ApplicationDbContext _context;

        public GetEquipmentByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EquipmentDetailsDto> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
        {
            var equipment = await _context.Equipments
                .AsNoTracking()
                .Where(e => e.Id == request.Id)
                .Select(e => new EquipmentDetailsDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Unit = e.Unit,
                    WeightKg = e.WeightKg,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.Name,
                    SubCategoryId = e.SubCategoryId,
                    SubCategoryName = e.SubCategory.Name,
                    InitialQuantity = e.InitialQuantity,
                    PurchasedQuantity = e.PurchasedQuantity,
                    QuantityAvailable = e.QuantityAvailable,
                    QuantityScrapped = e.QuantityScrapped,
                    QuantityInTransit = e.QuantityInTransit,

                    QuantityOnAllSites = e.SiteEquipments.Sum(se => se.Quantity),

                    // DODANE MAPOWANIE TABLICY BUDÓW:
                    SiteAllocations = e.SiteEquipments.Select(se => new EquipmentSiteAllocationDto
                    {
                        SiteId = se.SiteId,
                        SiteName = se.Site.Name,
                        QuantityOnSite = se.Quantity,
                        Address = se.Site.Address

                    }).ToList(),

                    PurchasePrice = e.PurchasePrice,
                    DayPrice = e.DayPrice,
                    IsActive = e.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (equipment == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono sprzętu o id: {request.Id}");
            }

            return equipment;
        }
    }
}