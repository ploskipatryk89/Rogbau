using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.HistoricalData.Dtos;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.HistoricalData.Queries.GetAll
{
    public class GetAllHistoricalDataQueryHandler
        : IRequestHandler<GetAllHistoricalDataQuery, PaginatedList<HistoricalDataDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public GetAllHistoricalDataQueryHandler(
            ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<PaginatedList<HistoricalDataDto>> Handle(
            GetAllHistoricalDataQuery request,
            CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Dane historyczne są dostępne tylko dla administratora.");

            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
                throw new ArgumentException("Data od nie może być późniejsza niż data do.");

            var query = _context.SiteEquipmentAllocations
                .AsNoTracking()
                .Include(x => x.Site)
                .Include(x => x.Equipment)
                .AsQueryable();

            if (request.SiteId.HasValue)
            {
                query = query.Where(x =>
                    x.SiteId == request.SiteId.Value);
            }

            if (request.EquipmentId.HasValue)
            {
                query = query.Where(x =>
                    x.EquipmentId == request.EquipmentId.Value);
            }

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value
                    .ToDateTime(TimeOnly.MinValue);

                query = query.Where(x =>
                    x.DateTo == null ||
                    x.DateTo >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value
                    .ToDateTime(TimeOnly.MaxValue);

                query = query.Where(x =>
                    x.DateFrom <= toDate);
            }

            var now = DateTime.UtcNow;

            DateTime? filterFrom = request.FromDate?
                .ToDateTime(TimeOnly.MinValue);

            DateTime? filterTo = request.ToDate?
                .ToDateTime(TimeOnly.MaxValue);

            var historicalDataQuery = query
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    SiteName = x.Site.Name,
                    x.EquipmentId,
                    EquipmentName = x.Equipment.Name,
                    x.Quantity,
                    x.DayPrice,

                    StartDate = filterFrom.HasValue &&
                                filterFrom.Value > x.DateFrom
                        ? filterFrom.Value
                        : x.DateFrom,

                    EndDate = filterTo.HasValue &&
                              filterTo.Value < (x.DateTo ?? now)
                        ? filterTo.Value
                        : (x.DateTo ?? now),

                    HasEndDate = request.ToDate.HasValue || x.DateTo.HasValue
                })
                .Where(x =>
                    x.StartDate <= now &&
                    x.EndDate >= x.StartDate)
                .OrderBy(x => x.EquipmentName)
                .ThenBy(x => x.Id)
                .Select(x => new HistoricalDataDto
                {
                    Id = x.Id,
                    SiteId = x.SiteId,
                    SiteName = x.SiteName,
                    EquipmentId = x.EquipmentId,
                    EquipmentName = x.EquipmentName,

                    FromDate = x.StartDate,
                    ToDate = x.HasEndDate ? (DateTime?)x.EndDate : null,

                    Quantity = x.Quantity,
                    DayPrice = x.DayPrice,

                    Days = Math.Max(
                        0,
                        (x.EndDate.Date - x.StartDate.Date).Days),

                    TotalEarned = x.Quantity * x.DayPrice * Math.Max(
                        0,
                        (x.EndDate.Date - x.StartDate.Date).Days)
                });

            return await PaginatedList<HistoricalDataDto>.CreateAsync(
                historicalDataQuery,
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                cancellationToken);
        }
    }
}
