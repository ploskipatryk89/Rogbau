using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public CreateCategoryCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Kategoriami zarządza administrator.");

            var name = request.Name.Trim();

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var category = new Category();

            if (await _context.Categories.AnyAsync(x => x.Id != category.Id &&
                x.Name.ToLower() == name.ToLower(), cancellationToken))
                throw new ArgumentException("Kategoria o tej nazwie już istnieje.");

            category.Name = name;
            _context.Categories.Add(category);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return category.Id;
        }
    }
}
