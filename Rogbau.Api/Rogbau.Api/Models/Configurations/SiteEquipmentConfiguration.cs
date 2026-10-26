using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class SiteEquipmentConfiguration : IEntityTypeConfiguration<SiteEquipment>
    {
        public void Configure(EntityTypeBuilder<SiteEquipment> builder)
        {
            builder.ToTable("SiteEquipments");

            builder.HasKey(se => se.Id);

            builder.Property(se => se.Quantity)
                .IsRequired();

            builder.HasOne(se => se.Site)
                .WithMany(s => s.EquipmentItems)
                .HasForeignKey(se => se.SiteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Poprawiony fragment:
            builder.HasOne(se => se.Equipment)
                .WithMany(e => e.SiteEquipments) // <--- Dodano parametr e.SiteEquipments
                .HasForeignKey(se => se.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}