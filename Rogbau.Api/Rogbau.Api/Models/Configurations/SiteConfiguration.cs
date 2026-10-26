using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class SiteConfiguration : IEntityTypeConfiguration<Site>
    {
        public void Configure(EntityTypeBuilder<Site> builder)
        {
            builder.ToTable("Sites");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(s => s.Name)
                .IsUnique();

            builder.Property(s => s.Address)
                .HasMaxLength(500);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasMany(s => s.EquipmentItems)
                .WithOne(e => e.Site)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
