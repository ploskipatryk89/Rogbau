using FluentValidation;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditTransfer
{
    public class EditTransferCommandValidator : AbstractValidator<EditTransferCommand>
    {
        public EditTransferCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.FromConstructionSiteId).NotEmpty().NotEqual(x => x.ToConstructionSiteId).WithMessage("Wybierz dwie różne budowy.");
            RuleFor(x => x.ToConstructionSiteId).NotEmpty().WithMessage("Wybierz budowę docelową.");
            RuleFor(x => x.Items).Cascade(CascadeMode.Stop).NotEmpty()
                .Must(items => items.Select(i => i.EquipmentId).Distinct().Count() == items.Count)
                .WithMessage("Dodaj pozycje bez powtarzającego się sprzętu.");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.EquipmentId).NotEmpty();
                item.RuleFor(x => x.Quantity).GreaterThan(0);
            });
        }
    }
}
