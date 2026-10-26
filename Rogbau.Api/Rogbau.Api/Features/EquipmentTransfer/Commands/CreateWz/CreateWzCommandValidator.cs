using FluentValidation;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreateWz
{
    public class CreateWzCommandValidator : AbstractValidator<CreateWzCommand>
    {
        public CreateWzCommandValidator()
        {
            RuleFor(x => x.ToConstructionSiteId).NotEmpty().WithMessage("Wybierz budowę.");

            RuleFor(x => x.Note).MaximumLength(1000).WithMessage("Uwagi mogą mieć maksymalnie 1000 znaków.");

            RuleFor(x => x.Items).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Dodaj przynajmniej jedną pozycję sprzętu.")
                .Must(items => items.Select(item => item.EquipmentId).Distinct().Count() == items.Count)
                .WithMessage("Sprzęt nie może się powtarzać.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Wybierz sprzęt.");
                item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Ilość musi być większa od 0.");
            });
        }
    }
}
