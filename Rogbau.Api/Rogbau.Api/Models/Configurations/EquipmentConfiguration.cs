using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Configurations
{
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("Equipments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Property(e => e.Unit)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("szt.");

            builder.Property(e => e.WeightKg)
                .IsRequired()
                .HasPrecision(18, 2);

            // Ceny i precyzja kwot finansowych
            builder.Property(e => e.PurchasePrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(e => e.DayPrice)
                .IsRequired()
                .HasPrecision(18, 4);

            // Relacja z Category (Wymagana)
            builder.HasOne(e => e.Category)
                .WithMany(c => c.Equipments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Zabrania usunięcia kategorii, jeśli jest przypisany do niej sprzęt

            // Relacja z SubCategory (Opcjonalna)
            builder.HasOne(e => e.SubCategory)
                .WithMany(sc => sc.Equipments)
                .HasForeignKey(e => e.SubCategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Jeśli podkategoria zostanie usunięta, pole w sprzęcie ustawi się na null

            
        }
    }
}