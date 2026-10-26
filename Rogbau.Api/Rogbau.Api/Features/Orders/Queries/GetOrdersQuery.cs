using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Orders.Dtos;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Orders.Queries;

public record GetOrdersQuery(Guid? Id = null) : IRequest<List<OrderDetailsDto>>;

public class GetOrdersQueryHandler(ApplicationDbContext context, ICurrentUserService user)
    : IRequestHandler<GetOrdersQuery, List<OrderDetailsDto>>
{
    public async Task<List<OrderDetailsDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Orders.AsNoTracking().AsQueryable();
        if (!user.IsInRole("Admin") && !user.IsInRole("Magazynier"))
        {
            if (!user.IsInRole("Kierownik Budowy") || string.IsNullOrEmpty(user.UserId))
                query = query.Where(x => false);
            else
                query = query.Where(x => context.SiteUsers.Any(s => s.SiteId == x.SiteId && s.UserId == user.UserId));
        }
        if (request.Id.HasValue)
            query = query.Where(x => x.Id == request.Id.Value);

        return await query.OrderByDescending(x => x.CreatedAt).Select(x => new OrderDetailsDto
        {
            Id = x.Id, OrderNumber = x.OrderNumber, SiteId = x.SiteId, SiteName = x.Site.Name,
            Note = x.Note, Date = x.CreatedAt, Status = x.Status,
            Items = x.Items.Select(i => new OrderDetailsItemDto
            {
                EquipmentId = i.EquipmentId, EquipmentName = i.Equipment.Name, Unit = i.Equipment.Unit,
                Quantity = i.Quantity, QuantityAvailable = i.Equipment.QuantityAvailable, WeightKg = i.Equipment.WeightKg,
                SentQuantity = context.EquipmentMovementItems.Where(m => m.EquipmentMovement.OrderId == x.Id &&
                    m.EquipmentId == i.EquipmentId && m.EquipmentMovement.Status != EquipmentMovementStatus.Cancelled)
                    .Sum(m => (int?)m.Quantity) ?? 0
            }).ToList(),
            Transfers = context.EquipmentMovements.Where(m => m.OrderId == x.Id).OrderByDescending(m => m.CreatedAt)
                .Select(m => new OrderMovementDto { Id = m.Id, Number = m.Number, Status = m.Status }).ToList()
        }).ToListAsync(cancellationToken);
    }
}
