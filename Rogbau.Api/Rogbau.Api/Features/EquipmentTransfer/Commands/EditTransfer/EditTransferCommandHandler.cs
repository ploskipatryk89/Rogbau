using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditTransfer
{
    public class EditTransferCommandHandler : IRequestHandler<EditTransferCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;
        private readonly MovementEditStockService _stock;

        public EditTransferCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
            _stock = new MovementEditStockService(context);
        }

        public async Task<Unit> Handle(EditTransferCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
            {
                throw new UnauthorizedAccessException("Przesunięcie może edytować tylko administrator.");
            }
            if (request.FromConstructionSiteId == request.ToConstructionSiteId)
            {
                throw new ArgumentException("Wybierz dwie różne budowy.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var transfer = await _context.EquipmentMovements.Include(e => e.Items)
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.Type == EquipmentMovementType.Transfer, cancellationToken);
            if (transfer == null) throw new KeyNotFoundException("Nie znaleziono przesunięcia.");
            if (transfer.Status != EquipmentMovementStatus.InTransit && transfer.Status != EquipmentMovementStatus.Completed)
            {
                throw new InvalidOperationException("Można edytować przesunięcie w transporcie lub zakończone.");
            }
            if (transfer.FromConstructionSiteId == null || transfer.ToConstructionSiteId == null)
            {
                throw new InvalidOperationException("Przesunięcie nie ma budowy źródłowej lub docelowej.");
            }
            var source = await _context.Sites.FirstOrDefaultAsync(s => s.Id == request.FromConstructionSiteId, cancellationToken);
            var target = await _context.Sites.FirstOrDefaultAsync(s => s.Id == request.ToConstructionSiteId, cancellationToken);
            if (source == null || target == null) throw new KeyNotFoundException("Nie znaleziono budowy.");
            if (target.Status == SiteStatus.Completed && target.Id != transfer.ToConstructionSiteId)
            {
                throw new InvalidOperationException("Nie można przesunąć sprzętu na zakończoną budowę.");
            }

            var equipmentIds = new List<Guid>();
            foreach (var item in transfer.Items)
            {
                if (equipmentIds.Contains(item.EquipmentId))
                {
                    throw new InvalidOperationException("Sprzęt powtarza się w zapisanym przesunięciu.");
                }
                equipmentIds.Add(item.EquipmentId);
            }
            foreach (var item in request.Items)
            {
                if (!equipmentIds.Contains(item.EquipmentId)) equipmentIds.Add(item.EquipmentId);
            }
            var equipments = await _context.Equipments.Where(e => equipmentIds.Contains(e.Id)).ToListAsync(cancellationToken);
            if (equipments.Count != equipmentIds.Count) throw new KeyNotFoundException("Nie znaleziono sprzętu.");

            Guid oldSourceId = transfer.FromConstructionSiteId.Value;
            Guid oldTargetId = transfer.ToConstructionSiteId.Value;
            var siteIds = new List<Guid> { oldSourceId };
            if (!siteIds.Contains(oldTargetId)) siteIds.Add(oldTargetId);
            if (!siteIds.Contains(source.Id)) siteIds.Add(source.Id);
            if (!siteIds.Contains(target.Id)) siteIds.Add(target.Id);
            DateTime date = DateTime.UtcNow;
            foreach (var equipment in equipments)
            {
                var oldItem = transfer.Items.FirstOrDefault(i => i.EquipmentId == equipment.Id);
                var newItem = request.Items.FirstOrDefault(i => i.EquipmentId == equipment.Id);
                int oldQuantity = 0;
                int newQuantity = 0;
                if (oldItem != null) oldQuantity = oldItem.Quantity;
                if (newItem != null) newQuantity = newItem.Quantity;
                if (!equipment.IsActive && newQuantity > oldQuantity)
                {
                    throw new InvalidOperationException("Nie można zwiększać przesunięcia nieaktywnego sprzętu.");
                }
                if (target.Status == SiteStatus.Completed && newQuantity > oldQuantity)
                {
                    throw new InvalidOperationException("Nie można zwiększać przesunięcia na zakończoną budowę.");
                }
                if (transfer.Status == EquipmentMovementStatus.InTransit)
                {
                    equipment.QuantityInTransit = _stock.CalculateStock(equipment.QuantityInTransit, newQuantity - oldQuantity);
                }

                // Każdą budowę korygujemy raz. To działa również przy zamianie kierunku A -> B na B -> A.
                foreach (Guid siteId in siteIds)
                {
                    long difference = 0;
                    if (siteId == oldSourceId) difference += oldQuantity;
                    if (siteId == source.Id) difference -= newQuantity;
                    if (transfer.Status == EquipmentMovementStatus.Completed)
                    {
                        if (siteId == oldTargetId) difference -= oldQuantity;
                        if (siteId == target.Id) difference += newQuantity;
                    }
                    await _stock.ChangeSiteStock(siteId, equipment, difference, date, cancellationToken);
                }
            }

            _context.EquipmentMovementItems.RemoveRange(transfer.Items);
            var newItems = new List<EquipmentMovementItem>();
            foreach (var item in request.Items)
            {
                var newItem = new EquipmentMovementItem();
                newItem.EquipmentMovementId = transfer.Id;
                newItem.EquipmentId = item.EquipmentId;
                newItem.Quantity = item.Quantity;
                newItems.Add(newItem);
            }
            _context.EquipmentMovementItems.AddRange(newItems);
            transfer.Items = newItems;
            transfer.FromConstructionSiteId = source.Id;
            transfer.ToConstructionSiteId = target.Id;
            transfer.Note = request.Note;
            if (transfer.Note != null) transfer.Note = transfer.Note.Trim();
            transfer.UpdatedAt = date;
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
