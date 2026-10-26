using FluentValidation;

namespace Rogbau.Api.Features.Suppliers.Commands.CreateSupplier
{
    public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
    {
        public CreateSupplierCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Nazwa dostawcy jest wymagana");
        }
    }
}
