using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Equipments.Dtos;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Equipments.Queries.GetAllEquipments
{
    public class GetAllEquipmentsQueryHandler : IRequestHandler<GetAllEquipmentsQuery, PaginatedList<EquipmentDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllEquipmentsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<EquipmentDto>> Handle(GetAllEquipmentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Equipments
               
                .AsNoTracking()
                .Include(e => e.Category)       // <--- TUTAJ BRAKOWAŁO INCLUDE DLA KATEGORII
                .Include(e => e.SubCategory)    // <--- TUTAJ BRAKOWAŁO INCLUDE DLA PODKATEGORII
                .AsQueryable();
                

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var search = request.SearchQuery.Trim().ToLower();
                query = query.Where(e => e.Name.ToLower().Contains(search));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(e => e.IsActive == request.IsActive.Value);
            }
            else
            {
                query = query.Where(e => e.IsActive == true);
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == request.CategoryId.Value);
            }

            if (request.SubCategoryId.HasValue)
            {
                query = query.Where(e => e.SubCategoryId == request.SubCategoryId.Value);
            }
            var equipmentQuery = query
                .OrderBy(e => e.Name)
                .ThenBy(e => e.Id)
                .Select(e => new EquipmentDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Unit = e.Unit,
                    WeightKg = e.WeightKg,
                    InitialQuantity = e.InitialQuantity,
                    QuantityAvailable = e.QuantityAvailable,
                    QuantityScrapped = e.QuantityScrapped,
                    PurchasedQuantity = e.PurchasedQuantity,
                    DayPrice = e.DayPrice,
                    IsActive = e.IsActive,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category != null
                        ? e.Category.Name
                        : string.Empty,
                    SubCategoryId = e.SubCategoryId,
                    SubCategoryName = e.SubCategory != null
                        ? e.SubCategory.Name
                        : null,
                    PurchasePrice = e.PurchasePrice
                });

            return await PaginatedList<EquipmentDto>.CreateAsync(
                equipmentQuery,
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                cancellationToken);
        }
    }
}