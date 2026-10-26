using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Categories.Commands.EditCategory
{
    public class EditCategoryCommandHandler : IRequestHandler<EditCategoryCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public EditCategoryCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(EditCategoryCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Kategoriami zarządza administrator.");

            var name = request.Name.Trim();

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
                throw new KeyNotFoundException("Nie znaleziono kategorii.");

            if (await _context.Categories.AnyAsync(x => x.Id != category.Id &&
                x.Name.ToLower() == name.ToLower(), cancellationToken))
                throw new ArgumentException("Kategoria o tej nazwie już istnieje.");

            category.Name = name;


            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
