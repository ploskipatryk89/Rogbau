using System.ComponentModel;

namespace Rogbau.Api.Models
{
    public class Equipment : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal WeightKg { get; set; } = 0m;

        // Kategoria (Wymagana)
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Podkategoria (Opcjonalna - Np. dla szalunków)
        public Guid? SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        // Stany ilościowe
        public int InitialQuantity { get; set; }     // Stan pierwotny (bilans otwarcia)
        public int PurchasedQuantity { get; set; }   // Dokupione w trakcie
        public int QuantityAvailable { get; set; }   // Dostępne na magazynie
        public int QuantityScrapped { get; set; }    // Złom / Zniszczone
        public int QuantityInTransit { get; set; }

        // Ceny i Stawki (Wymagane)
        public decimal PurchasePrice { get; set; }   // Cena zakupu (nie-nullable)
        public decimal DayPrice { get; set; }        // Stawka dzienna amortyzacji/wynajmu

        public bool IsActive { get; set; } = true;

        public List<SiteEquipment> SiteEquipments { get; set; } = new List<SiteEquipment>();
    }
}