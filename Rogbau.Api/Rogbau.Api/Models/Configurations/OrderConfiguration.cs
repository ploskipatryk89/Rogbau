using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired();

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Property(o => o.Status)
                .HasConversion<string>();

            builder.HasOne(o => o.Site)
    .WithMany()
    .HasForeignKey(o => o.SiteId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Items)
    .WithOne(oi => oi.Order)
    .HasForeignKey(oi => oi.OrderId)
    .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
