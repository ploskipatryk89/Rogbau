using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditWz
{
    public class EditWzCommandHandler : IRequestHandler<EditWzCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;
        private readonly MovementEditStockService _stock;

        public EditWzCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
            _stock = new MovementEditStockService(context);
        }

        public async Task<Unit> Handle(EditWzCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
            {
                throw new UnauthorizedAccessException("WZ może edytować tylko administrator.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var wz = await _context.EquipmentMovements.Include(e => e.Items)
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.Type == EquipmentMovementType.WZ, cancellationToken);
            if (wz == null)
            {
                throw new KeyNotFoundException("Nie znaleziono WZ.");
            }
            if (wz.Status != EquipmentMovementStatus.InTransit && wz.Status != EquipmentMovementStatus.Completed)
            {
                throw new InvalidOperationException("Można edytować WZ w transporcie lub zakończone.");
            }
            if (wz.ToConstructionSiteId == null)
            {
                throw new InvalidOperationException("WZ nie ma budowy docelowej.");
            }
            var target = await _context.Sites.FirstOrDefaultAsync(s => s.Id == request.ToConstructionSiteId, cancellationToken);
            if (target == null)
            {
                throw new KeyNotFoundException("Nie znaleziono budowy docelowej.");
            }
            if (target.Status == SiteStatus.Completed && target.Id != wz.ToConstructionSiteId)
            {
                throw new InvalidOperationException("Nie można przenieść WZ na zakończoną budowę.");
            }
            await ValidateOrder(wz, request, cancellationToken);

            var equipmentIds = new List<Guid>();
            foreach (var item in wz.Items)
            {
                if (equipmentIds.Contains(item.EquipmentId))
                {
                    throw new InvalidOperationException("Sprzęt powtarza się w zapisanym WZ.");
                }
                equipmentIds.Add(item.EquipmentId);
            }
            foreach (var item in request.Items)
            {
                if (!equipmentIds.Contains(item.EquipmentId))
                {
                    equipmentIds.Add(item.EquipmentId);
                }
            }
            var equipments = await _context.Equipments.Where(e => equipmentIds.Contains(e.Id)).ToListAsync(cancellationToken);
            if (equipments.Count != equipmentIds.Count)
            {
                throw new KeyNotFoundException("Nie znaleziono sprzętu.");
            }

            DateTime date = DateTime.UtcNow;
            Guid oldSiteId = wz.ToConstructionSiteId.Value;
            Guid? oldOrderId = wz.OrderId;
            foreach (var equipment in equipments)
            {
                var oldItem = wz.Items.FirstOrDefault(i => i.EquipmentId == equipment.Id);
                var newItem = request.Items.FirstOrDefault(i => i.EquipmentId == equipment.Id);
                int oldQuantity = 0;
                int newQuantity = 0;
                if (oldItem != null) oldQuantity = oldItem.Quantity;
                if (newItem != null) newQuantity = newItem.Quantity;
                if (!equipment.IsActive && newQuantity > oldQuantity)
                {
                    throw new InvalidOperationException("Nie można zwiększać wydania nieaktywnego sprzętu.");
                }
                if (target.Status == SiteStatus.Completed && newQuantity > oldQuantity)
                {
                    throw new InvalidOperationException("Nie można zwiększać wydania na zakończoną budowę.");
                }

                // Oddajemy magazynowi starą ilość i odejmujemy nową.
                int difference = newQuantity - oldQuantity;
                equipment.QuantityAvailable = _stock.CalculateStock(equipment.QuantityAvailable, -difference);
                if (wz.Status == EquipmentMovementStatus.InTransit)
                {
                    equipment.QuantityInTransit = _stock.CalculateStock(equipment.QuantityInTransit, difference);
                }
                else if (oldSiteId == target.Id)
                {
                    await _stock.ChangeSiteStock(target.Id, equipment, difference, date, cancellationToken);
                }
                else
                {
                    await _stock.ChangeSiteStock(oldSiteId, equipment, -oldQuantity, date, cancellationToken);
                    await _stock.ChangeSiteStock(target.Id, equipment, newQuantity, date, cancellationToken);
                }
            }

            _context.EquipmentMovementItems.RemoveRange(wz.Items);
            var newItems = new List<EquipmentMovementItem>();
            foreach (var item in request.Items)
            {
                var newItem = new EquipmentMovementItem();
                newItem.EquipmentMovementId = wz.Id;
                newItem.EquipmentId = item.EquipmentId;
                newItem.Quantity = item.Quantity;
                newItems.Add(newItem);
            }
            _context.EquipmentMovementItems.AddRange(newItems);
            wz.Items = newItems;
            wz.ToConstructionSiteId = target.Id;
            wz.OrderId = request.OrderId;
            wz.Note = request.Note;
            if (wz.Note != null) wz.Note = wz.Note.Trim();
            wz.UpdatedAt = date;
            await _context.SaveChangesAsync(cancellationToken);

            // Po zapisie pozycje WZ są już aktualne, więc można przeliczyć zamówienie.
            await UpdateOrderStatus(oldOrderId, cancellationToken);
            if (request.OrderId != oldOrderId)
            {
                await UpdateOrderStatus(request.OrderId, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }

        private async Task ValidateOrder(EquipmentMovement wz, EditWzCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId == null) return;
            if (request.OrderId != wz.OrderId)
            {
                throw new InvalidOperationException("Przy edycji można zachować istniejące zamówienie lub odłączyć WZ.");
            }
            var order = await _context.Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
            if (order == null) throw new KeyNotFoundException("Nie znaleziono zamówienia.");
            if (order.SiteId != request.ToConstructionSiteId || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("WZ nie pasuje do zamówienia. Odłącz zamówienie, aby zapisać korektę.");
            }
            foreach (var item in request.Items)
            {
                var orderedItem = order.Items.FirstOrDefault(i => i.EquipmentId == item.EquipmentId);
                if (orderedItem == null)
                {
                    throw new InvalidOperationException("Sprzętu nie ma w zamówieniu. Odłącz zamówienie, aby zapisać korektę.");
                }
                var otherItems = await _context.EquipmentMovementItems.Where(i =>
                    i.EquipmentMovement.OrderId == order.Id && i.EquipmentMovementId != wz.Id &&
                    i.EquipmentMovement.Status != EquipmentMovementStatus.Cancelled && i.EquipmentId == item.EquipmentId)
                    .ToListAsync(cancellationToken);
                long sent = item.Quantity;
                foreach (var otherItem in otherItems) sent += otherItem.Quantity;
                if (sent > orderedItem.Quantity)
                {
                    throw new InvalidOperationException("Ilość przekracza zamówienie. Odłącz zamówienie, aby zapisać korektę.");
                }
            }
        }

        private async Task UpdateOrderStatus(Guid? orderId, CancellationToken cancellationToken)
        {
            if (orderId == null) return;
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            if (order == null || order.Status == OrderStatus.Cancelled) return;
            var sentItems = await _context.EquipmentMovementItems.Where(i =>
                i.EquipmentMovement.OrderId == order.Id && i.EquipmentMovement.Status != EquipmentMovementStatus.Cancelled)
                .ToListAsync(cancellationToken);
            bool complete = true;
            foreach (var orderedItem in order.Items)
            {
                long sent = 0;
                foreach (var sentItem in sentItems)
                {
                    if (sentItem.EquipmentId == orderedItem.EquipmentId) sent += sentItem.Quantity;
                }
                if (sent < orderedItem.Quantity) complete = false;
            }
            order.Status = OrderStatus.PartiallySent;
            if (sentItems.Count == 0) order.Status = OrderStatus.Pending;
            else if (complete) order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }
}
