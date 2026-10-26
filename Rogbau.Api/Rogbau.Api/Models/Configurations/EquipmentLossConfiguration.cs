using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Models.Configurations
{
    public class EquipmentLossConfiguration : IEntityTypeConfiguration<EquipmentLoss>
    {
        public void Configure(EntityTypeBuilder<EquipmentLoss> builder)
        {
            builder.HasOne(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ReportedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(e => e.LossDate)
                .IsRequired();

          

            builder.Property(e => e.Note)
                .HasMaxLength(250);

            builder.HasMany(e => e.Items)
                .WithOne(i => i.EquipmentLoss)
                .HasForeignKey(i => i.EquipmentLossId);
        }
    }
}
