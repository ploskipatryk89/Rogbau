using FluentValidation;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.ConfirmEquipmentMovement
{
    public class ConfirmEquipmentMovementCommandValidator : AbstractValidator<ConfirmEquipmentMovementCommand>
    {
        public ConfirmEquipmentMovementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Wybierz dokument.");

            When(x => x.Items != null, () =>
            {
                RuleForEach(x => x.Items).ChildRules(item =>
                {
                    item.RuleFor(x => x.EquipmentId).NotEmpty().WithMessage("Wybierz sprzęt.");
                    item.RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Przyjęta ilość nie może być ujemna.");
                    item.RuleFor(x => x.ScrappedQuantity).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.Quantity)
                        .WithMessage("Ilość złomu musi być nieujemna i nie może przekraczać przyjętej ilości.");
                });
            });
        }
    }
}
