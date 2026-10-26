using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class EquipmentMovementConfiguration
        : IEntityTypeConfiguration<EquipmentMovement>
    {
        public void Configure(EntityTypeBuilder<EquipmentMovement> builder)
        {
            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromConstructionSite)
                .WithMany()
                .HasForeignKey(x => x.FromConstructionSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ToConstructionSite)
                .WithMany()
                .HasForeignKey(x => x.ToConstructionSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AddedBy)
                .WithMany()
                .HasForeignKey(x => x.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.EquipmentMovement)
                .HasForeignKey(x => x.EquipmentMovementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Number)
                .IsUnique();
        }
    }
}
