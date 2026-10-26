using System.Security.Claims;

namespace Rogbau.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string roleName)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.IsInRole(roleName) ?? false;
        }

        // Pobiera listę wszystkich ról zalogowanego użytkownika
        public IEnumerable<string> GetRoles()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return Enumerable.Empty<string>();

            return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
        }
    }
}

