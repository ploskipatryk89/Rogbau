using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class EquipmentMovementItemConfiguration
        : IEntityTypeConfiguration<EquipmentMovementItem>
    {
        public void Configure(EntityTypeBuilder<EquipmentMovementItem> builder)
        {
            builder.HasOne(x => x.Equipment)
                .WithMany()
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}