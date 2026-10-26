using FluentValidation;

namespace Rogbau.Api.Features.Equipments.Commands.EditEquipment
{
    public class EditEquipmentCommandValidator : AbstractValidator<EditEquipmentCommand>
    {
        public EditEquipmentCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Wybierz sprzęt.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Podaj nazwę do 200 znaków.");
            RuleFor(x => x.Unit).NotEmpty().MaximumLength(20).WithMessage("Podaj jednostkę do 20 znaków.");
            RuleFor(x => x.DayPrice).GreaterThan(0).WithMessage("Stawka dzienna musi być większa od 0.");
            RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0).WithMessage("Waga nie może być ujemna.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Wybierz kategorię.");
        }
    }
}
