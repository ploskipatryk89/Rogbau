using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;

namespace Rogbau.Api.Features.EquipmentTransfer.Services
{
    public class NumberGenerator
    {
        private readonly ApplicationDbContext _context;

        public NumberGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateNumberAsync(
            EquipmentMovementType type,
            CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = type.ToString();

            var prefixWithYear = $"{prefix}/{year}/";

            var lastNumber = await _context.EquipmentMovements
                .Where(x => x.Type == type &&
                            x.Number.StartsWith(prefixWithYear))
                .OrderByDescending(x => x.Number)
                .Select(x => x.Number)
                .FirstOrDefaultAsync(cancellationToken);

            var next = 1;

            if (lastNumber is not null)
            {
                var lastSequence = int.Parse(lastNumber.Split('/').Last());
                next = lastSequence + 1;
            }

            return $"{prefix}/{year}/{next:D5}";
        }
    }
}