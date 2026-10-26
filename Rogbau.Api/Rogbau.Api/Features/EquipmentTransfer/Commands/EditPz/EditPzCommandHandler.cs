using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Commands.EditPz
{
    public class EditPzCommandHandler : IRequestHandler<EditPzCommand, Unit>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public EditPzCommandHandler(ApplicationDbContext context, ICurrentUserService user)
        {
            _context = context;
            _user = user;
        }

        public async Task<Unit> Handle(EditPzCommand request, CancellationToken cancellationToken)
        {
            if (!_user.IsInRole("Admin"))
            {
                throw new UnauthorizedAccessException("PZ może edytować tylko administrator.");
            }

            // Wszystkie zmiany zapisujemy razem. Błąd wycofa całą edycję.
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

            var pz = await _context.EquipmentMovements
                .Include(e => e.Items)
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.Type == EquipmentMovementType.PZ, cancellationToken);

            if (pz == null)
            {
                throw new KeyNotFoundException("Nie znaleziono PZ.");
            }
            if (pz.Status != EquipmentMovementStatus.InTransit && pz.Status != EquipmentMovementStatus.Completed)
            {
                throw new InvalidOperationException("Można edytować PZ w transporcie lub zakończone.");
            }
            if (pz.FromConstructionSiteId == null)
            {
                throw new InvalidOperationException("PZ nie ma budowy źródłowej.");
            }

            var newSite = await _context.Sites
                .FirstOrDefaultAsync(s => s.Id == request.FromConstructionSiteId, cancellationToken);
            if (newSite == null)
            {
                throw new KeyNotFoundException("Nie znaleziono budowy.");
            }

            ValidateReceivedQuantities(pz, request);

            // Potrzebujemy sprzętu ze starego PZ i z nowych pozycji.
            var equipmentIds = new List<Guid>();
            foreach (var oldItem in pz.Items)
            {
                if (equipmentIds.Contains(oldItem.EquipmentId))
                {
                    throw new InvalidOperationException("Sprzęt powtarza się w zapisanym PZ.");
                }
                equipmentIds.Add(oldItem.EquipmentId);
            }
            foreach (var newItem in request.Items)
            {
                if (!equipmentIds.Contains(newItem.EquipmentId))
                {
                    equipmentIds.Add(newItem.EquipmentId);
                }
            }

            var equipments = await _context.Equipments
                .Where(e => equipmentIds.Contains(e.Id))
                .ToListAsync(cancellationToken);
            if (equipments.Count != equipmentIds.Count)
            {
                throw new KeyNotFoundException("Nie znaleziono sprzętu.");
            }

            Guid oldSiteId = pz.FromConstructionSiteId.Value;
            Guid newSiteId = request.FromConstructionSiteId;
            DateTime correctionDate = DateTime.UtcNow;

            foreach (var equipment in equipments)
            {
                var oldItem = pz.Items.FirstOrDefault(i => i.EquipmentId == equipment.Id);
                var newItem = request.Items.FirstOrDefault(i => i.EquipmentId == equipment.Id);

                if (pz.Status == EquipmentMovementStatus.InTransit)
                {
                    await EditInTransit(equipment, oldItem, newItem, oldSiteId, newSiteId, correctionDate, cancellationToken);
                }
                else
                {
                    await EditCompleted(equipment, oldItem, newItem, oldSiteId, newSiteId, correctionDate, cancellationToken);
                }
            }

            // Stare dane były potrzebne do obliczeń. Dopiero teraz je zastępujemy.
            _context.EquipmentMovementItems.RemoveRange(pz.Items);
            var newItems = new List<EquipmentMovementItem>();
            foreach (var item in request.Items)
            {
                var newItem = new EquipmentMovementItem();
                newItem.EquipmentMovementId = pz.Id;
                newItem.EquipmentId = item.EquipmentId;
                newItem.Quantity = item.Quantity;
                newItem.ScrappedQuantity = item.ScrappedQuantity;
                newItem.ReceivedQuantity = item.ReceivedQuantity;
                newItem.ReceivedScrappedQuantity = item.ReceivedScrappedQuantity;
                newItems.Add(newItem);
            }
            _context.EquipmentMovementItems.AddRange(newItems);
            pz.Items = newItems;
            pz.FromConstructionSiteId = newSiteId;
            pz.Note = request.Note;
            if (pz.Note != null)
            {
                pz.Note = pz.Note.Trim();
            }
            pz.UpdatedAt = correctionDate;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }

        private void ValidateReceivedQuantities(EquipmentMovement pz, EditPzCommand request)
        {
            foreach (var item in request.Items)
            {
                if (pz.Status == EquipmentMovementStatus.Completed)
                {
                    if (item.ReceivedQuantity == null || item.ReceivedScrappedQuantity == null)
                    {
                        throw new ArgumentException("Podaj ilość przyjętą i przyjęty złom.");
                    }
                    if (item.ReceivedQuantity < 0 || item.ReceivedScrappedQuantity < 0 ||
                        item.ReceivedScrappedQuantity > item.ReceivedQuantity)
                    {
                        throw new ArgumentException("Niepoprawna ilość przyjęta lub przyjęty złom.");
                    }
                }
                else
                {
                    if (item.ReceivedQuantity != null || item.ReceivedScrappedQuantity != null)
                    {
                        throw new ArgumentException("PZ w transporcie nie może mieć ilości przyjętych.");
                    }
                }
            }
        }

        private async Task EditInTransit(Equipment equipment, EquipmentMovementItem? oldItem,
            EditPzItemDto? newItem, Guid oldSiteId, Guid newSiteId, DateTime date, CancellationToken cancellationToken)
        {
            int oldQuantity = 0;
            int newQuantity = 0;
            if (oldItem != null)
            {
                oldQuantity = oldItem.Quantity;
            }
            if (newItem != null)
            {
                newQuantity = newItem.Quantity;
            }

            // Przykład: było 5, teraz 3. Transport maleje o 2.
            int difference = newQuantity - oldQuantity;
            equipment.QuantityInTransit = CalculateStock(equipment.QuantityInTransit, difference);
            await CorrectSites(equipment, oldSiteId, newSiteId, oldQuantity, newQuantity, date, cancellationToken);
        }

        private async Task EditCompleted(Equipment equipment, EquipmentMovementItem? oldItem,
            EditPzItemDto? newItem, Guid oldSiteId, Guid newSiteId, DateTime date, CancellationToken cancellationToken)
        {
            int oldReceived = 0;
            int oldScrapped = 0;
            int newReceived = 0;
            int newScrapped = 0;

            if (oldItem != null)
            {
                // Starszy dokument może nie mieć osobno zapisanej ilości przyjętej.
                oldReceived = oldItem.Quantity;
                oldScrapped = oldItem.ScrappedQuantity;
                if (oldItem.ReceivedQuantity != null)
                {
                    oldReceived = oldItem.ReceivedQuantity.Value;
                }
                if (oldItem.ReceivedScrappedQuantity != null)
                {
                    oldScrapped = oldItem.ReceivedScrappedQuantity.Value;
                }
            }
            if (newItem != null)
            {
                // Obecność tych wartości sprawdziliśmy w ValidateReceivedQuantities.
                newReceived = newItem.ReceivedQuantity.GetValueOrDefault();
                newScrapped = newItem.ReceivedScrappedQuantity.GetValueOrDefault();
            }

            int oldGoodQuantity = oldReceived - oldScrapped;
            int newGoodQuantity = newReceived - newScrapped;
            int goodQuantityDifference = newGoodQuantity - oldGoodQuantity;
            int scrappedQuantityDifference = newScrapped - oldScrapped;

            equipment.QuantityAvailable = CalculateStock(equipment.QuantityAvailable, goodQuantityDifference);
            equipment.QuantityScrapped = CalculateStock(equipment.QuantityScrapped, scrappedQuantityDifference);
            await CorrectSites(equipment, oldSiteId, newSiteId, oldReceived, newReceived, date, cancellationToken);
        }

        // W jednym miejscu sprawdzamy ujemny stan i zakres int, bez checked.
        private int CalculateStock(int currentQuantity, int difference)
        {
            long newQuantity = (long)currentQuantity + difference;
            if (newQuantity < 0)
            {
                throw new InvalidOperationException("Korekta daje ujemny stan sprzętu. Sprawdź późniejsze dokumenty.");
            }
            if (newQuantity > int.MaxValue)
            {
                throw new InvalidOperationException("Ilość sprzętu jest zbyt duża.");
            }
            return (int)newQuantity;
        }

        private async Task CorrectSites(Equipment equipment, Guid oldSiteId, Guid newSiteId,
            int oldQuantity, int newQuantity, DateTime date, CancellationToken cancellationToken)
        {
            if (oldSiteId == newSiteId)
            {
                // Ta sama budowa: zmieniamy tylko różnicę.
                int difference = oldQuantity - newQuantity;
                await ChangeSiteStock(oldSiteId, equipment, difference, date, cancellationToken);
            }
            else
            {
                // Inna budowa: starej oddajemy, z nowej zabieramy.
                await ChangeSiteStock(oldSiteId, equipment, oldQuantity, date, cancellationToken);
                await ChangeSiteStock(newSiteId, equipment, -newQuantity, date, cancellationToken);
            }
        }

        private async Task ChangeSiteStock(Guid siteId, Equipment equipment, int difference,
            DateTime date, CancellationToken cancellationToken)
        {
            if (difference == 0)
            {
                return;
            }
            var stock = await _context.SiteEquipments
                .FirstOrDefaultAsync(s => s.SiteId == siteId && s.EquipmentId == equipment.Id, cancellationToken);
            int currentQuantity = 0;
            if (stock != null)
            {
                currentQuantity = stock.Quantity;
            }
            int newQuantity = CalculateStock(currentQuantity, difference);

            var allocations = await _context.SiteEquipmentAllocations
                .Where(a => a.SiteId == siteId && a.EquipmentId == equipment.Id && a.DateTo == null)
                .OrderBy(a => a.DateFrom)
                .ThenBy(a => a.Id)
                .ToListAsync(cancellationToken);

            long allocatedQuantity = 0;
            foreach (var allocation in allocations)
            {
                allocatedQuantity += allocation.Quantity;
            }
            if (allocatedQuantity > currentQuantity)
            {
                throw new InvalidOperationException("Historia przydziałów przekracza stan budowy.");
            }

            if (difference > 0)
            {
                if (stock == null)
                {
                    stock = new SiteEquipment();
                    stock.SiteId = siteId;
                    stock.EquipmentId = equipment.Id;
                    _context.SiteEquipments.Add(stock);
                }
                stock.Quantity = newQuantity;

                // Dodatkowa ilość jest na budowie od dnia korekty.
                var allocation = new SiteEquipmentAllocation();
                allocation.SiteId = siteId;
                allocation.EquipmentId = equipment.Id;
                allocation.Quantity = difference;
                allocation.DateFrom = date;
                allocation.DayPrice = equipment.DayPrice;
                _context.SiteEquipmentAllocations.Add(allocation);
            }
            else
            {
                if (stock == null)
                {
                    throw new InvalidOperationException("Nie znaleziono sprzętu na budowie.");
                }
                CloseAllocations(allocations, -difference, date);
                stock.Quantity = newQuantity;
                if (stock.Quantity == 0)
                {
                    _context.SiteEquipments.Remove(stock);
                }
            }
        }

        // Kończymy naliczanie dla ilości, która schodzi z budowy.
        // Nie zmieniamy wcześniejszych zamkniętych okresów.
        private void CloseAllocations(List<SiteEquipmentAllocation> allocations, int quantityToRemove, DateTime date)
        {
            foreach (var allocation in allocations)
            {
                if (quantityToRemove == 0)
                {
                    break;
                }
                if (quantityToRemove >= allocation.Quantity)
                {
                    allocation.DateTo = date;
                    quantityToRemove -= allocation.Quantity;
                }
                else
                {
                    // Część przydziału zostaje na budowie, a część kończymy.
                    var closedAllocation = new SiteEquipmentAllocation();
                    closedAllocation.SiteId = allocation.SiteId;
                    closedAllocation.EquipmentId = allocation.EquipmentId;
                    closedAllocation.Quantity = quantityToRemove;
                    closedAllocation.DateFrom = allocation.DateFrom;
                    closedAllocation.DateTo = date;
                    closedAllocation.DayPrice = allocation.DayPrice;
                    _context.SiteEquipmentAllocations.Add(closedAllocation);

                    allocation.Quantity -= quantityToRemove;
                    quantityToRemove = 0;
                }
            }
            // Bilans otwarcia może zawierać sprzęt bez historii przydziałów.
        }
    }
}
