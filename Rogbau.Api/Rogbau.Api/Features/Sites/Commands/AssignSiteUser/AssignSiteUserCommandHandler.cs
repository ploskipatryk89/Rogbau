using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Commands.AssignSiteUser
{
    public class AssignSiteUserCommandHandler : IRequestHandler<AssignSiteUserCommand>
    {
        private readonly ApplicationDbContext _context;

        public AssignSiteUserCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(
      AssignSiteUserCommand request,
      CancellationToken cancellationToken)
        {
            var siteExists = await _context.Sites
                .AnyAsync(x => x.Id == request.SiteId, cancellationToken);

            if (!siteExists)
                throw new KeyNotFoundException("Nie znaleziono budowy.");

            var alreadyAssigned = await _context.SiteUsers
                .AnyAsync(
                    x => x.SiteId == request.SiteId &&
                         x.UserId == request.UserId,
                    cancellationToken);

            if (alreadyAssigned)
                throw new InvalidOperationException(
                    "Ten kierownik jest już przypisany do budowy.");

            _context.SiteUsers.Add(new SiteUser
            {
                SiteId = request.SiteId,
                UserId = request.UserId
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
