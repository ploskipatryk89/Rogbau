using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Common.Pagination;

namespace Rogbau.Api.Features.EquipmentTransfer.Queries.GetAllTransfers
{
    public class GetAllTransfersQueryHandler
        : IRequestHandler<GetAllTransfersQuery, PaginatedList<TransferDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public GetAllTransfersQueryHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<PaginatedList<TransferDto>> Handle(
            GetAllTransfersQuery request,
            CancellationToken cancellationToken)
        {
            var query = MovementVisibility.ForUser(_context.EquipmentMovements.AsNoTracking().AsQueryable(), _context, _user);

            if (request.Type.HasValue)
            {
                query = query.Where(e => e.Type == request.Type.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(e =>
                    e.Number.Contains(request.Search));
            }

            if (request.DateFrom.HasValue)
            {
                var dateFrom = request.DateFrom.Value.ToDateTime(TimeOnly.MinValue);

                query = query.Where(e =>
                    e.CreatedAt >= dateFrom);
            }

            if (request.DateTo.HasValue)
            {
                var dateTo = request.DateTo.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue);

                query = query.Where(e =>
                    e.CreatedAt < dateTo);
            }

            var transferQuery =  query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new TransferDto
                {
                    Id = e.Id,
                    Number = e.Number,

                    FromName = e.FromConstructionSite == null
                        ? "Magazyn"
                        : e.FromConstructionSite.Name,

                    ToName = e.ToConstructionSite == null
                        ? "Magazyn"
                        : e.ToConstructionSite.Name,

                    Date = e.CreatedAt,
                    Status = e.Status,
                    Type = e.Type
                });

            return await PaginatedList<TransferDto>.CreateAsync(
                transferQuery,
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                cancellationToken);
        }
    }
}
