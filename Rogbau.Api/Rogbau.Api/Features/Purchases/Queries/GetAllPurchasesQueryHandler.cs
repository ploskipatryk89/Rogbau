using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Purchases.Dtos;
using Rogbau.Api.Models.Data;
using System.Reflection.Metadata.Ecma335;

namespace Rogbau.Api.Features.Purchases.Queries
{
    public class GetAllPurchasesQueryHandler : IRequestHandler<GetAllPurchasesQuery, PaginatedList<PurchaseListDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllPurchasesQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<PurchaseListDto>> Handle(GetAllPurchasesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Purchases.AsNoTracking().AsQueryable();

            // 2. Mapowanie i pobranie danych
            var purchaseQuery = query
    .OrderByDescending(p => p.PurchaseDate)
    .Select(p => new PurchaseListDto
    {
        Id = p.Id,
        SupplierId = p.SupplierId,
        Date = p.PurchaseDate,
        SupplierName = p.Supplier.Name,

        TotalItemsCount = p.Items.Count,
        TotalQuantity = p.Items.Sum(i => i.Quantity),

        TotalPrice = p.Items.Sum(i => i.Quantity * i.PurchasePrice),

        Items = p.Items.Select(i => new PurchaseItemDto
        {
            EquipmentName = i.Equipment.Name,
            EquipmentId = i.EquipmentId,
            Unit = i.Equipment.Unit,
            Quantity = i.Quantity,
            PurchaseItemPrice = i.PurchasePrice,
            PurchaseItemTotalPrice = i.PurchasePrice * i.Quantity
        }).ToList()
    });
    

            return await PaginatedList<PurchaseListDto>.CreateAsync(
                purchaseQuery,
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                cancellationToken);


        }

       
    }


}
