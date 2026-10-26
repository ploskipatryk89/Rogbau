namespace Rogbau.Api.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get;         }
        bool IsAuthenticated { get; }

        bool IsInRole(string roleName);
        IEnumerable<string> GetRoles();
    }
}
