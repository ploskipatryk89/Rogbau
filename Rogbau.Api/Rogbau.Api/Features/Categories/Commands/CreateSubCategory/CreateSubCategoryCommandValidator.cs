using FluentValidation;

namespace Rogbau.Api.Features.Categories.Commands.CreateSubCategory
{
    public class CreateSubCategoryCommandValidator : AbstractValidator<CreateSubCategoryCommand>
    {
        public CreateSubCategoryCommandValidator()
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Nazwa jest wymagana.")
                .Must(name => name.Trim().Length <= 100).WithMessage("Nazwa może mieć maksymalnie 100 znaków.");

            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Wybierz kategorię nadrzędną.");
        }
    }
}
