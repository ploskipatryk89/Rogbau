using FluentValidation;

namespace Rogbau.Api.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Wybierz poprawną kategorię lub podkategorię.");
        }
    }
}
