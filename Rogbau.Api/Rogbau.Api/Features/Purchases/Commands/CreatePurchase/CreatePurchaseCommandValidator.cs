using FluentValidation;

namespace Rogbau.Api.Features.Purchases.Commands
{
    public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
    {
        public CreatePurchaseCommandValidator()
        {
            RuleFor(x => x.SupplierId).NotEmpty().WithMessage("Wybierz dostawcę.");

            RuleFor(x => x.PurchaseDate).NotEmpty().WithMessage("Data zakupu jest wymagana.");

            RuleFor(x => x.Items).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Dodaj przynajmniej jedną pozycję sprzętu.")
                .Must(items => items.Select(item => item.EquipmentId).Distinct().Count() == items.Count)
                .WithMessage("Sprzęt nie może się powtarzać.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Wybierz sprzęt.");
                item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Ilość musi być większa od 0.");
                item.RuleFor(x => x.PurchasePrice).Cascade(CascadeMode.Stop)
                    .GreaterThan(0).WithMessage("Cena jednostkowa musi być większa od 0.")
                    .Must(price => decimal.Round(price, 2) == price).WithMessage("Cena może mieć maksymalnie dwa miejsca po przecinku.");
            });
        }
    }
}
