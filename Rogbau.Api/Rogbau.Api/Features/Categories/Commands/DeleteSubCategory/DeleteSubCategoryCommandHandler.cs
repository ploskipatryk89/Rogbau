using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Categories.Commands.DeleteSubCategory
{
    public class DeleteSubCategoryCommandHandler : IRequestHandler<DeleteSubCategoryCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public DeleteSubCategoryCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(DeleteSubCategoryCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Kategoriami zarządza administrator.");

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var category = await _context.SubCategories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
                throw new KeyNotFoundException("Nie znaleziono podkategorii.");

            if (await _context.Equipments.AnyAsync(x => x.SubCategoryId == category.Id, cancellationToken))
                throw new ArgumentException("Podkategoria jest używana przez sprzęt.");

            _context.SubCategories.Remove(category);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
