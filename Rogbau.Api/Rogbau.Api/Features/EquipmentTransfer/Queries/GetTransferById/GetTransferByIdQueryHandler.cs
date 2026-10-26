using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Models.Enums;
using Rogbau.Api.Services;
using Rogbau.Api.Features.EquipmentTransfer.Services;

namespace Rogbau.Api.Features.EquipmentTransfer.Queries.GetTransferById
{
    public class GetTransferByIdQueryHandler
        : IRequestHandler<GetTransferByIdQuery, TransferDetailsDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetTransferByIdQueryHandler(
            ApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<TransferDetailsDto> Handle(
            GetTransferByIdQuery request,
            CancellationToken cancellationToken)
        {
            var movement = await MovementVisibility.ForUser(
                    _context.EquipmentMovements.AsNoTracking(), _context, _currentUserService)
                .Where(x => x.Id == request.Id)
                .Select(x => new
                {
                    Transfer = new TransferDetailsDto
                    {
                        Id = x.Id,
                        Number = x.Number,
                        Note = x.Note,
                        OrderId = x.OrderId,

                        FromConstructionSiteId = x.FromConstructionSiteId,
                        FromName = x.FromConstructionSite == null
                            ? "Magazyn"
                            : x.FromConstructionSite.Name,

                        ToConstructionSiteId = x.ToConstructionSiteId,
                        ToName = x.ToConstructionSite == null
                            ? "Magazyn"
                            : x.ToConstructionSite.Name,

                        TotalWeight = x.Items.Sum(
                            i => i.Equipment.WeightKg * (i.ReceivedQuantity ?? i.Quantity)
                        ),

                        Date = x.CreatedAt,
                        Status = x.Status,
                        Type = x.Type,

                        Items = x.Items
                            .Select(item => new TransferDetailsItemDto
                            {
                                EquipmentId = item.EquipmentId,
                                EquipmentName = item.Equipment.Name,
                                Unit = item.Equipment.Unit,
                                Quantity = item.Quantity,
                                ScrappedQuantity = item.ScrappedQuantity,
                                ReceivedQuantity = item.ReceivedQuantity,
                                ReceivedScrappedQuantity = item.ReceivedScrappedQuantity
                            })
                            .ToList()
                    },

                    AddedByUserId = x.AddedByUserId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (movement is null)
                throw new KeyNotFoundException("Nie znaleziono dokumentu lub nie masz do niego dostępu.");

            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException(
                    "Użytkownik nie jest zalogowany.");

            var isAssignedToTargetSite = false;

            if (movement.Transfer.ToConstructionSiteId.HasValue)
            {
                isAssignedToTargetSite = await _context.SiteUsers
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.SiteId ==
                                movement.Transfer.ToConstructionSiteId.Value &&
                            x.UserId == userId,
                        cancellationToken
                    );
            }

            var canConfirm = _currentUserService.IsInRole("Kierownik Budowy") &&
                isAssignedToTargetSite;
            if (_currentUserService.IsInRole("Admin") && movement.Transfer.Type == EquipmentMovementType.Transfer &&
                movement.AddedByUserId != userId)
                canConfirm = true;

            var targetSiteCompleted = await _context.Sites.AnyAsync(s =>
                s.Id == movement.Transfer.ToConstructionSiteId && s.Status == SiteStatus.Completed, cancellationToken);

            if (movement.Transfer.Type == EquipmentMovementType.PZ)
            {
                movement.Transfer.CanConfirm = movement.Transfer.Status == EquipmentMovementStatus.InTransit &&
                    (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Magazynier"));
                return movement.Transfer;
            }

            movement.Transfer.CanConfirm =
                movement.Transfer.Status ==
                    EquipmentMovementStatus.InTransit &&
                (
                    movement.Transfer.Type ==
                        EquipmentMovementType.WZ ||
                    movement.Transfer.Type ==
                        EquipmentMovementType.Transfer
                ) &&
                canConfirm && !targetSiteCompleted;

            return movement.Transfer;
        }
    }
}

