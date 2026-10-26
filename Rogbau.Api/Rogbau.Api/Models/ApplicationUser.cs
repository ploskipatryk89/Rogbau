using Microsoft.AspNetCore.Identity;


namespace Rogbau.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public bool MustChangePassword { get; set; } = true;

        public ICollection<SiteUser> Sites { get; set; } = new List<SiteUser>();
    }
}
