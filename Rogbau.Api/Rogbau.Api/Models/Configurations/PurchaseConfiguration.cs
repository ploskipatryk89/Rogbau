using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PurchaseNumber)
                .IsRequired()
                .HasMaxLength(50); // Generowany w handlerze

            builder.Property(p => p.PurchaseDate)
                .IsRequired();

            builder.Property(p => p.TotalQuantity)
                .IsRequired(); // Wyliczany w handlerze

            // Relacja z dostawcą
            builder.HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId) // Shadow property jeśli brak SupplierId w klasie Purchase
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja z pozycjami (Dostawa ma wiele pozycji)
            builder.HasMany(p => p.Items)
                .WithOne(pi => pi.Purchase)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade); // Usunięcie dostawy kasuje jej pozycje
        }

    }
}
