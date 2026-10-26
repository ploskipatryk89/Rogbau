using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.Sites.Commands.FinishSite
{
    public class FinishSiteCommandHandler : IRequestHandler<FinishSiteCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly Rogbau.Api.Services.ICurrentUserService _user;

        public FinishSiteCommandHandler(ApplicationDbContext context, Rogbau.Api.Services.ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(FinishSiteCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
                throw new UnauthorizedAccessException("Budowę może zakończyć tylko administrator.");
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var site = await _context.Sites
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (site == null)
            {
                throw new KeyNotFoundException("Nie znaleziono budowy");

            }

            if (site.Status == SiteStatus.Completed)
            {
                throw new ConflictException("Budowa zostala juz zakonczona");
            }

            var hasEquipment = await _context.SiteEquipments
    .AnyAsync(
        e => e.SiteId == site.Id && e.Quantity > 0,
        cancellationToken);

            if (hasEquipment)
            {
                throw new ConflictException(
                    "Do budowy są przypisane jeszcze sprzęty. Zwróć je, aby zakończyć budowę."
                );
            }

            var hasPendingTransport = await _context.EquipmentMovements.AnyAsync(m =>
                m.Status == EquipmentMovementStatus.InTransit &&
                (m.ToConstructionSiteId == site.Id || m.FromConstructionSiteId == site.Id), cancellationToken);
            if (hasPendingTransport)
                throw new ConflictException("Budowa ma sprzęt w transporcie. Najpierw potwierdź odbiór dokumentów.");

            if (site.StartDate.HasValue && request.EndDate < site.StartDate.Value)
            {
                throw new ConflictException(
                    "Data zakończenia budowy nie może być wcześniejsza od daty rozpoczęcia."
                );
            }


            site.EndDate = request.EndDate;
            site.Status = SiteStatus.Completed;
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
