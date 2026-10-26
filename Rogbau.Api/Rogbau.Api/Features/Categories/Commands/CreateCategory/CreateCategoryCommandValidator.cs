using FluentValidation;

namespace Rogbau.Api.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Nazwa jest wymagana.")
                .Must(name => name.Trim().Length <= 100).WithMessage("Nazwa może mieć maksymalnie 100 znaków.");
        }
    }
}
