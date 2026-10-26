using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions.Equipments;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Commands.CreateSite
{
    public class CreateSiteCommandHandler
        : IRequestHandler<CreateSiteCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreateSiteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(
            CreateSiteCommand request,
            CancellationToken cancellationToken)
        {
            var siteExists = await _context.Sites
                .AnyAsync(
                    s => s.Name == request.Name,
                    cancellationToken);

            if (siteExists)
            {
                throw new AlreadyExistsException(
                    $"Budowa o nazwie {request.Name} juz istnieje");
            }

            var newSite = new Site
            {
                Name = request.Name,
                Address = request.Address,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status
            };

            _context.Sites.Add(newSite);

            // Przypisanie wybranych kierowników do budowy
            foreach (var userId in request.UserIds)
            {
                var siteUser = new SiteUser
                {
                    Site = newSite,
                    UserId = userId
                };

                _context.SiteUsers.Add(siteUser);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return newSite.Id;
        }
    }
}