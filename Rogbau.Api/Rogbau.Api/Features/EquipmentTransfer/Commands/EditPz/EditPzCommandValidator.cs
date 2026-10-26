using FluentValidation;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditPz
{
    public class EditPzCommandValidator : AbstractValidator<EditPzCommand>
    {
        public EditPzCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.FromConstructionSiteId).NotEmpty().WithMessage("Wybierz budowę.");
            RuleFor(x => x.Items).Cascade(CascadeMode.Stop).NotEmpty()
                .Must(items => items.Select(i => i.EquipmentId).Distinct().Count() == items.Count)
                .WithMessage("Dodaj pozycje bez powtarzającego się sprzętu.");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.EquipmentId).NotEmpty();
                item.RuleFor(x => x.Quantity).GreaterThan(0);
                item.RuleFor(x => x.ScrappedQuantity).GreaterThanOrEqualTo(0)
                    .LessThanOrEqualTo(x => x.Quantity);
            });
        }
    }
}
