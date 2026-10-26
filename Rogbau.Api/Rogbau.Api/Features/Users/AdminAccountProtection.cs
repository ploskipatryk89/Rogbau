using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Users;

internal static class AdminAccountProtection
{
    // Call inside a serializable transaction so concurrent changes cannot remove all admins.
    public static async Task EnsureCanRemoveAdminAsync(
        ApplicationDbContext context, string userId, CancellationToken cancellationToken)
    {
        var adminIds = await (from userRole in context.UserRoles
                              join role in context.Roles on userRole.RoleId equals role.Id
                              where role.Name == DbInitializer.AdminRole
                              select userRole.UserId).ToListAsync(cancellationToken);

        if (adminIds.Contains(userId) && !adminIds.Any(id => id != userId))
            throw new ConflictException("Nie można odebrać roli ani usunąć ostatniego administratora. Najpierw nadaj rolę Admin innemu użytkownikowi.");
    }
}
