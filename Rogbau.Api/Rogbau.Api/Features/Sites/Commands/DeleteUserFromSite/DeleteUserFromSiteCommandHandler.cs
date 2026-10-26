using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Commands.DeleteUserFromSite
{
    public class DeleteUserFromSiteCommandHandler
        : IRequestHandler<DeleteUserFromSiteCommand, Unit>
    {
        private readonly ApplicationDbContext _context;

        public DeleteUserFromSiteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
            DeleteUserFromSiteCommand request,
            CancellationToken cancellationToken)
        {
            var siteUser = await _context.SiteUsers
                .FirstOrDefaultAsync(
                    x => x.SiteId == request.SiteId &&
                         x.UserId == request.UserId,
                    cancellationToken);

            if (siteUser is null)
            {
                throw new KeyNotFoundException(
                    "Użytkownik nie jest przypisany do tej budowy.");
            }

            _context.SiteUsers.Remove(siteUser);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}