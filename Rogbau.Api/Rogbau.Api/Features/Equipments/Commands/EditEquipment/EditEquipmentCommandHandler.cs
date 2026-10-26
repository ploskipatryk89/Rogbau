using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions.Equipments;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Equipments.Commands.EditEquipment
{
    public class EditEquipmentCommandHandler : IRequestHandler<EditEquipmentCommand, Unit>
    {
        private readonly ApplicationDbContext _context;

        public EditEquipmentCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EditEquipmentCommand request, CancellationToken cancellationToken)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken) ||
                (request.SubCategoryId.HasValue && !await _context.SubCategories.AnyAsync(s =>
                    s.Id == request.SubCategoryId && s.CategoryId == request.CategoryId, cancellationToken)))
                throw new ArgumentException("Wybierz poprawną kategorię i należącą do niej podkategorię.");
            var equipment = await _context.Equipments
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (equipment == null)
            {
                throw new KeyNotFoundException($"Sprzęt o Id {request.Id} nie istnieje.");
            }

            // Walidacja czy nowa nazwa nie koliduje z innym sprzętem w bazie
            var nameExists = await _context.Equipments
    .AnyAsync(e =>
        e.Name.ToLower() == request.Name.Trim().ToLower()
        && e.Id != request.Id
        && e.IsActive,
        cancellationToken);

            if (nameExists)
            {
                throw new NameAlreadyExistsException(request.Name);
            }

            equipment.Name = request.Name.Trim();
            equipment.Unit = request.Unit;
            equipment.WeightKg = request.WeightKg;
            equipment.CategoryId = request.CategoryId;
            equipment.SubCategoryId = request.SubCategoryId;
            equipment.PurchasePrice = request.PurchasePrice;
            equipment.DayPrice = request.DayPrice;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
