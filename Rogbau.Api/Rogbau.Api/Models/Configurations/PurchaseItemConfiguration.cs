using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseItem> builder)
        {
            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.Quantity)
                .IsRequired();

            // Relacja do sprzętu/towaru (Wiele pozycji może wskazywać na ten sam sprzęt)
            builder.HasOne(pi => pi.Equipment)
                .WithMany() // Jeśli w Equipment nie ma kolekcji PurchaseItems
                .HasForeignKey(pi => pi.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict); // Blokuje usunięcie sprzętu, jeśli jest na jakiejś dostawie
        }
    }
}
