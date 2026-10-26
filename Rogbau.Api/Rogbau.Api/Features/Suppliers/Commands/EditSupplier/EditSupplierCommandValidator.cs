using FluentValidation;

namespace Rogbau.Api.Features.Suppliers.Commands.EditSupplier;

public class EditSupplierCommandValidator : AbstractValidator<EditSupplierCommand>
{
    public EditSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Nazwa dostawcy jest wymagana.");
    }
}
