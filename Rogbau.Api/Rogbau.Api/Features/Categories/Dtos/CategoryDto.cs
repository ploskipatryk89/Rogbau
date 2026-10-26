using System.Security.Cryptography;

namespace Rogbau.Api.Features.Categories.Dtos
{
    public record CategoryDto
    {
       public  Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public List<SubCategoryDto> SubCategories { get; set; } = new List<SubCategoryDto>();

    }
 
    


}
