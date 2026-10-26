using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions;
using Rogbau.Api.Models.Data;
using System.Data;

namespace Rogbau.Api.Features.Equipments.Commands.DeleteEquipment
{
    public class DeleteEquipmentCommandHandler : IRequestHandler<DeleteEquipmentCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteEquipmentCommandHandler> _logger;

        public DeleteEquipmentCommandHandler(ApplicationDbContext context, ILogger<DeleteEquipmentCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteEquipmentCommand request, CancellationToken cancellationToken)
        {
            // Powiązania nie mogą pojawić się pomiędzy sprawdzeniem a usunięciem.
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            var equipment = await _context.Equipments
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (equipment == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono sprzętu o ID {request.Id}");
            }

            bool hasHistory = await _context.PurchaseItems.AnyAsync(x => x.EquipmentId == request.Id, cancellationToken)
                || await _context.Orders.AnyAsync(x => x.Items.Any(item => item.EquipmentId == request.Id), cancellationToken)
                || await _context.EquipmentMovementItems.AnyAsync(x => x.EquipmentId == request.Id, cancellationToken)
                || await _context.SiteEquipments.AnyAsync(x => x.EquipmentId == request.Id, cancellationToken)
                || await _context.SiteEquipmentAllocations.AnyAsync(x => x.EquipmentId == request.Id, cancellationToken);

            if (hasHistory)
            {
                throw new ConflictException("Nie można trwale usunąć tego sprzętu, ponieważ jest powiązany z zakupami, zamówieniami lub historią magazynową i budowami. Usunięcie naruszyłoby historię dokumentów.");
            }

            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation($"Usunieto produkt o Id: {equipment.Id} i nazwie {equipment.Name}");

            return Unit.Value;
        }
    }


}
