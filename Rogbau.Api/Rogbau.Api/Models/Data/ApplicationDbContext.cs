using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rogbau.Api.Models.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Purchase> Purchases { get; set; }

        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteEquipment> SiteEquipments { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> Recipients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<EquipmentMovement> EquipmentMovements { get; set; }
        public DbSet<EquipmentMovementItem> EquipmentMovementItems { get; set; }
        public DbSet<SiteEquipmentAllocation> SiteEquipmentAllocations { get; set; }

        public DbSet<SiteUser> SiteUsers { get; set; }
      
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
