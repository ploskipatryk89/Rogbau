using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Sites.Dtos
{
    public record SiteDetailsDto(
    Guid Id,
    string Name,
    string? Address,
    DateOnly? StartDate,
    DateOnly? EndDate,
    SiteStatus Status,
    List<SiteEquipmentDto> Equipment,
        List<SiteManagerDto> Managers
);
}
