namespace Rogbau.Api.Models
{
    public class SubCategory : BaseEntity
    {
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
