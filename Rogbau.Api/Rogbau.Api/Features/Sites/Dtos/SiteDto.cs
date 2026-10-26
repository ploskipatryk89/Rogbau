using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Sites.Dtos
{
    public record SiteDto(
     Guid Id,
     string Name,
     string? Address,
     DateOnly? StartDate,
     DateOnly? EndDate,
     SiteStatus Status,
     int TotalEquipmentCount // Opcjonalnie: suma pozycji na budowie
 );


}
