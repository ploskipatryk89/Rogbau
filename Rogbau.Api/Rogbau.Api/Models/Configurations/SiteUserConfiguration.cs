using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class SiteUserConfiguration : IEntityTypeConfiguration<SiteUser>
    {
        public void Configure(EntityTypeBuilder<SiteUser> builder)
        {
            builder.HasKey(x => new { x.SiteId, x.UserId });

            builder.HasOne(x => x.Site)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.SiteId);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Sites)
                .HasForeignKey(x => x.UserId);
        }
    }
}
