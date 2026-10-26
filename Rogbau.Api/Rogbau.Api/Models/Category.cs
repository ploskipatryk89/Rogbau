namespace Rogbau.Api.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
