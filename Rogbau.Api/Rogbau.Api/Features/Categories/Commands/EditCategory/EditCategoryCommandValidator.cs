using FluentValidation;

namespace Rogbau.Api.Features.Categories.Commands.EditCategory
{
    public class EditCategoryCommandValidator : AbstractValidator<EditCategoryCommand>
    {
        public EditCategoryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Wybierz poprawną kategorię lub podkategorię.");

            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Nazwa jest wymagana.")
                .Must(name => name.Trim().Length <= 100).WithMessage("Nazwa może mieć maksymalnie 100 znaków.");
        }
    }
}
