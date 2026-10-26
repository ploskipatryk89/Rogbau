using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Sites.Commands.StartSite
{
    public class StartSiteCommandHandler : IRequestHandler<StartSiteCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly Rogbau.Api.Services.ICurrentUserService _user;

        public StartSiteCommandHandler(ApplicationDbContext context, Rogbau.Api.Services.ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(StartSiteCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin") && !(_user.IsInRole("Kierownik Budowy") &&
                await _context.SiteUsers.AnyAsync(s => s.SiteId == request.Id && s.UserId == _user.UserId, cancellationToken)))
                throw new UnauthorizedAccessException("Możesz rozpoczynać tylko przypisane do Ciebie budowy.");
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (request.StartDate == default)
                throw new ArgumentException("Data rozpoczęcia jest wymagana.");
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (site == null)
            {
                throw new KeyNotFoundException("Nie znaleziono budowy");

            }

            if (site.Status == Models.Enums.SiteStatus.Completed)
            {
                throw new ConflictException("Budowa zostala juz zakonczona");
            }

            if (site.GetCurrentStatus(today) == Models.Enums.SiteStatus.InProgress)
            {
                throw new ConflictException("Budowa zostala juz rozpoczeta");
            }

            site.StartDate = request.StartDate;
            site.Status = request.StartDate <= today
                ? Models.Enums.SiteStatus.InProgress : Models.Enums.SiteStatus.Planned;
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
