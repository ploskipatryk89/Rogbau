using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Dashboard.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, object>
    {
        private const int LowStockThreshold = 5;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetDashboardQueryHandler(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<object> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Zaloguj się, aby zobaczyć pulpit.");

            if (_currentUser.IsInRole("Admin"))
                return await GetAdminDashboard(cancellationToken);
            if (_currentUser.IsInRole("Magazynier"))
                return await GetWarehousemanDashboard(cancellationToken);
            if (_currentUser.IsInRole("Kierownik Budowy"))
                return await GetSiteManagerDashboard(request.SiteId, cancellationToken);

            throw new UnauthorizedAccessException("Nie masz dostępu do pulpitu.");
        }

        private async Task<AdminDashboardDto> GetAdminDashboard(CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var activeSites = await _context.Sites.CountAsync(s => s.Status != SiteStatus.Completed &&
                (s.StartDate.HasValue ? s.StartDate.Value <= today : s.Status == SiteStatus.InProgress), ct);

            return new AdminDashboardDto
            {
                ActiveSiteQuantity = activeSites,
                EquipmentAvailable = await GetEquipmentAvailable(ct),
                EquipmentsOnAllSites = await _context.SiteEquipments.SumAsync(s => (int?)s.Quantity, ct) ?? 0,
                RecentlyDocuments = await GetRecentDocuments(_context.EquipmentMovements.AsNoTracking(), ct),
                RecentlyOrders = await GetRecentOrders(ct),
                LowEquipmentStocks = await GetLowEquipmentStocks(ct)
            };
        }

        private async Task<WarehousemanDashboardDto> GetWarehousemanDashboard(CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var movements = _context.EquipmentMovements.AsNoTracking();
            var monthly = movements.Where(m => m.CreatedAt >= monthStart && m.CreatedAt < monthEnd &&
                m.Status != EquipmentMovementStatus.Cancelled && m.Status != EquipmentMovementStatus.Draft);

            return new WarehousemanDashboardDto
            {
                EquipmentAvailable = await GetEquipmentAvailable(ct),
                WzQuantityThisMonth = await monthly.CountAsync(m => m.Type == EquipmentMovementType.WZ, ct),
                PzQuantityThisMonth = await monthly.CountAsync(m => m.Type == EquipmentMovementType.PZ, ct),
                RecentlyDocuments = await GetRecentDocuments(movements, ct),
                RecentlyOrders = await GetRecentOrders(ct),
                LowEquipmentStocks = await GetLowEquipmentStocks(ct)
            };
        }

        private async Task<SiteManagerDashboardDto> GetSiteManagerDashboard(Guid? siteId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
                throw new UnauthorizedAccessException("Nie można ustalić użytkownika.");

            var sites = await _context.Sites.AsNoTracking()
                .Where(s => _context.SiteUsers.Any(su => su.SiteId == s.Id && su.UserId == _currentUser.UserId))
                .OrderBy(s => s.Name)
                .Select(s => new ManagerSiteDto { Id = s.Id, SiteName = s.Name })
                .ToListAsync(ct);

            if (siteId.HasValue && !sites.Any(s => s.Id == siteId.Value))
                throw new UnauthorizedAccessException("Nie masz dostępu do tej budowy.");
            if (sites.Count == 0)
                return new SiteManagerDashboardDto();

            var selectedSiteId = siteId ?? sites[0].Id;
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var movements = _context.EquipmentMovements.AsNoTracking()
                .Where(m => m.FromConstructionSiteId == selectedSiteId || m.ToConstructionSiteId == selectedSiteId);
            var monthly = movements.Where(m => m.CreatedAt >= monthStart && m.CreatedAt < monthEnd &&
                m.Status != EquipmentMovementStatus.Cancelled && m.Status != EquipmentMovementStatus.Draft);

            return new SiteManagerDashboardDto
            {
                ManagerSites = sites,
                SelectedSiteId = selectedSiteId,
                Status = await _context.Sites.Where(s => s.Id == selectedSiteId).Select(s => s.Status).SingleAsync(ct),
                SiteEquipmentQuantity = await _context.SiteEquipments.Where(s => s.SiteId == selectedSiteId)
                    .SumAsync(s => (int?)s.Quantity, ct) ?? 0,
                WzQuantityThisMonth = await monthly.CountAsync(m => m.Type == EquipmentMovementType.WZ, ct),
                PzQuantityThisMonth = await monthly.CountAsync(m => m.Type == EquipmentMovementType.PZ, ct),
                RecentlyDocuments = await GetRecentDocuments(movements, ct)
            };
        }

        private async Task<int> GetEquipmentAvailable(CancellationToken ct)
        {
            return await _context.Equipments.Where(e => e.IsActive).SumAsync(e => (int?)e.QuantityAvailable, ct) ?? 0;
        }

        private async Task<List<RecentlyDocumentDto>> GetRecentDocuments(IQueryable<EquipmentMovement> movements, CancellationToken ct)
        {
            return await movements.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Number).Take(5)
                .Select(m => new RecentlyDocumentDto
                {
                    Id = m.Id,
                    Number = m.Number,
                    From = m.FromConstructionSite == null ? "Magazyn główny" : m.FromConstructionSite.Name,
                    To = m.ToConstructionSite == null ? "Magazyn główny" : m.ToConstructionSite.Name,
                    Status = m.Status,
                    Date = DateOnly.FromDateTime(m.CreatedAt)
                }).ToListAsync(ct);
        }

        private async Task<List<RecentlyOrderDto>> GetRecentOrders(CancellationToken ct)
        {
            return await _context.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.OrderNumber).Take(5)
                .Select(o => new RecentlyOrderDto
                {
                    Id = o.Id, Number = o.OrderNumber, SiteName = o.Site.Name, Status = o.Status
                }).ToListAsync(ct);
        }

        private async Task<List<LowEquipmentStockDto>> GetLowEquipmentStocks(CancellationToken ct)
        {
            return await _context.Equipments.AsNoTracking()
                .Where(e => e.IsActive && e.QuantityAvailable <= LowStockThreshold)
                .OrderBy(e => e.QuantityAvailable).ThenBy(e => e.Name).Take(5)
                .Select(e => new LowEquipmentStockDto { Id = e.Id, Name = e.Name, Quantity = e.QuantityAvailable })
                .ToListAsync(ct);
        }
    }
}
