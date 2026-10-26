using MediatR;

namespace Rogbau.Api.Features.Categories.Commands.CreateSubCategory
{
    public class CreateSubCategoryCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
    }
}
