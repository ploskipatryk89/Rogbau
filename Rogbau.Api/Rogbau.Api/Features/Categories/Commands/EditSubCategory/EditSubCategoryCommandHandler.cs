using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Categories.Commands.EditSubCategory
{
    public class EditSubCategoryCommandHandler : IRequestHandler<EditSubCategoryCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public EditSubCategoryCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(EditSubCategoryCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Kategoriami zarządza administrator.");

            var name = request.Name.Trim();

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var category = await _context.SubCategories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
                throw new KeyNotFoundException("Nie znaleziono podkategorii.");

            var parentId = category.CategoryId;
            if (!await _context.Categories.AnyAsync(x => x.Id == parentId, cancellationToken))
                throw new ArgumentException("Wybierz kategorię nadrzędną.");

            if (await _context.SubCategories.AnyAsync(x => x.CategoryId == parentId && x.Id != category.Id &&
                x.Name.ToLower() == name.ToLower(), cancellationToken))
                throw new ArgumentException("Podkategoria o tej nazwie już istnieje.");

            category.Name = name;


            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
