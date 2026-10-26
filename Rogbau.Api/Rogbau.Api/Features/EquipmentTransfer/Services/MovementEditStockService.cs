using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.EquipmentTransfer.Services
{
    // Wspólne obliczenia dla edycji WZ i przesunięć.
    public class MovementEditStockService
    {
        private readonly ApplicationDbContext _context;

        public MovementEditStockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CalculateStock(int currentQuantity, long difference)
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

        public async Task ChangeSiteStock(Guid siteId, Equipment equipment, long difference,
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
                .OrderBy(a => a.DateFrom).ThenBy(a => a.Id).ToListAsync(cancellationToken);
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
                var allocation = new SiteEquipmentAllocation();
                allocation.SiteId = siteId;
                allocation.EquipmentId = equipment.Id;
                allocation.Quantity = (int)difference;
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
                CloseAllocations(allocations, (int)-difference, date);
                stock.Quantity = newQuantity;
                if (stock.Quantity == 0)
                {
                    _context.SiteEquipments.Remove(stock);
                }
            }
        }

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
        }
    }
}

