using FluentValidation;

namespace Rogbau.Api.Features.Categories.Commands.DeleteSubCategory
{
    public class DeleteSubCategoryCommandValidator : AbstractValidator<DeleteSubCategoryCommand>
    {
        public DeleteSubCategoryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Wybierz poprawną kategorię lub podkategorię.");
        }
    }
}
