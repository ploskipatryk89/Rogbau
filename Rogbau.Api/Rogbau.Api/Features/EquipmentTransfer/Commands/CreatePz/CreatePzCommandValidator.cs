using FluentValidation;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreatePz
{
    public class CreatePzCommandValidator : AbstractValidator<CreatePzCommand>
    {
        public CreatePzCommandValidator()
        {
            RuleFor(x => x.FromConstructionSiteId).NotEmpty().WithMessage("Wybierz budowę.");

            RuleFor(x => x.Note).MaximumLength(1000).WithMessage("Uwagi mogą mieć maksymalnie 1000 znaków.");

            RuleFor(x => x.Items).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Dodaj przynajmniej jedną pozycję sprzętu.")
                .Must(items => items.Select(item => item.EquipmentId).Distinct().Count() == items.Count)
                .WithMessage("Sprzęt nie może się powtarzać.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Wybierz sprzęt.");
                item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Ilość musi być większa od 0.");
                item.RuleFor(x => x.QuantityScrapped).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.Quantity)
                    .WithMessage("Ilość złomu musi być nieujemna i nie może przekraczać zwracanej ilości.");
            });
        }
    }
}
