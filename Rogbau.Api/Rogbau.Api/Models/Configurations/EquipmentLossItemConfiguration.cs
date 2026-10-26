using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class EquipmentLossItemConfiguration : IEntityTypeConfiguration<EquipmentLossItem>
    {
        public void Configure(EntityTypeBuilder<EquipmentLossItem> builder)
        {


            builder.HasOne(e => e.EquipmentLoss)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.EquipmentLossId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(e => e.Equipment)
                .WithMany()
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Property(e => e.LossType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.Quantity)
                .IsRequired();

            builder.Property(e => e.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            

            
        }
    }
}
