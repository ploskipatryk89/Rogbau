using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Equipments.Commands.CreateEquipment
{
    public class CreateEquipmentCommandValidator : AbstractValidator<CreateEquipmentCommand>
    {
        private readonly ApplicationDbContext _context;
        public CreateEquipmentCommandValidator(ApplicationDbContext context)
        {
            _context = context;
            RuleFor(c => c.WeightKg).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Unit).MaximumLength(20);
            RuleFor(c => c.CategoryId).MustAsync((id, ct) =>
                _context.Categories.AnyAsync(c => c.Id == id, ct)).WithMessage("Wybierz poprawną kategorię.");
            RuleFor(c => c).MustAsync((c, ct) => !c.SubCategoryId.HasValue
                ? Task.FromResult(true)
                : _context.SubCategories.AnyAsync(s => s.Id == c.SubCategoryId && s.CategoryId == c.CategoryId, ct))
                .WithMessage("Podkategoria musi należeć do wybranej kategorii.");
            RuleFor(c => c.Name)
    .NotEmpty()
    .WithMessage("Nazwa jest wymagana")
    .MaximumLength(200)
    .WithMessage("Nazwa może mieć maksymalnie 200 znaków")
    .MustAsync(BeUniqueName)
    .WithMessage("Aktywny sprzęt o tej nazwie już istnieje");

            RuleFor(c => c.Unit)
                .NotEmpty().WithMessage("Jednostka miary jest wymagana");


            RuleFor(c => c.DayPrice)
                .Must(c => c > 0).WithMessage("Cena musi byc wieksza od 0");

}

        private async Task<bool> BeUniqueName(
       string name,
       CancellationToken cancellationToken)
        {
            var trimmedName = name.Trim();

            return !await _context.Equipments
                .AnyAsync(
                    e => e.Name == trimmedName && e.IsActive,
                    cancellationToken);
        }
    }
}
