using MediatR;

namespace Rogbau.Api.Features.Categories.Commands.DeleteSubCategory
{
    public class DeleteSubCategoryCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
    }
}
