using MediatR;

namespace Rogbau.Api.Features.Categories.Commands.EditCategory
{
    public class EditCategoryCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
