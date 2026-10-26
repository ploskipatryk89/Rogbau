using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public DeleteCategoryCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Kategoriami zarządza administrator.");

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
                throw new KeyNotFoundException("Nie znaleziono kategorii.");

            if (await _context.Equipments.AnyAsync(x => x.CategoryId == category.Id, cancellationToken) ||
                await _context.SubCategories.AnyAsync(x => x.CategoryId == category.Id, cancellationToken))
                throw new ArgumentException("Kategoria zawiera sprzęt lub podkategorie. Najpierw zmień kategorię sprzętu i usuń podkategorie.");

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
