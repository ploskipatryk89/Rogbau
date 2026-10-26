using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class SiteEquipmentAllocationConfiguration
        : IEntityTypeConfiguration<SiteEquipmentAllocation>
    {
        public void Configure(
            EntityTypeBuilder<SiteEquipmentAllocation> builder)
        {
            builder.HasOne(x => x.Site)
                .WithMany()
                .HasForeignKey(x => x.SiteId);

            builder.HasOne(x => x.Equipment)
                .WithMany()
                .HasForeignKey(x => x.EquipmentId);

            builder.Property(x => x.DayPrice)
                .HasPrecision(18, 4);
        }
    }
}
