using Rogbau.Api.Models;

public class SiteUser
{
    public Guid SiteId { get; set; }
    public Site Site { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}