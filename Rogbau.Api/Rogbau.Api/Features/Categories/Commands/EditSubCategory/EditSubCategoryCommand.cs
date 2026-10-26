using MediatR;

namespace Rogbau.Api.Features.Categories.Commands.EditSubCategory
{
    public class EditSubCategoryCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
