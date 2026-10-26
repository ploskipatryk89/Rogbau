using FluentValidation;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.CreateTransfer
{
    public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
    {
        public CreateTransferCommandValidator()
        {
            RuleFor(x => x.FromConstructionSiteId).NotEmpty().WithMessage("Wybierz budowę.");

            RuleFor(x => x.ToConstructionSiteId).NotEmpty().WithMessage("Wybierz budowę.");

            RuleFor(x => x.ToConstructionSiteId).NotEqual(x => x.FromConstructionSiteId)
                .WithMessage("Budowa źródłowa i docelowa nie mogą być takie same.");

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
