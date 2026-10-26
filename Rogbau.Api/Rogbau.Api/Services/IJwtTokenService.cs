using Rogbau.Api.Models;

namespace Rogbau.Api.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
    }
}
