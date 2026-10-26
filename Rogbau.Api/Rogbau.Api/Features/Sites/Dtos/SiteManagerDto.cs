using Microsoft.AspNetCore.Mvc;

namespace Rogbau.Api.Features.Sites.Dtos
{
    public record SiteManagerDto
    {
        public string Id { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
    }
}
