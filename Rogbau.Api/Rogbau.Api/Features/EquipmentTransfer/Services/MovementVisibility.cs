using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Services;

public static class MovementVisibility
{
    public static IQueryable<EquipmentMovement> ForUser(
        IQueryable<EquipmentMovement> query, ApplicationDbContext context, ICurrentUserService user)
    {
        if (user.IsInRole("Admin") || user.IsInRole("Magazynier"))
            return query;

        if (!user.IsInRole("Kierownik Budowy") || string.IsNullOrEmpty(user.UserId))
            return query.Where(x => false);

        var userId = user.UserId;
        var siteIds = context.SiteUsers.Where(x => x.UserId == userId).Select(x => x.SiteId);
        return query.Where(x =>
            (x.FromConstructionSiteId.HasValue && siteIds.Contains(x.FromConstructionSiteId.Value)) ||
            (x.ToConstructionSiteId.HasValue && siteIds.Contains(x.ToConstructionSiteId.Value)));
    }
}
