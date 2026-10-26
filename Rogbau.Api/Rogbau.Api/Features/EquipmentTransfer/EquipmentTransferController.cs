using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Equipments.Commands;
using Rogbau.Api.Features.Equipments.Dtos;
using Rogbau.Api.Features.Equipments.Queries.GetAllEquipments;
using Rogbau.Api.Features.EquipmentTransfer.Commands.ConfirmEquipmentMovement;
using Rogbau.Api.Features.EquipmentTransfer.Commands.CreatePz;
using Rogbau.Api.Features.EquipmentTransfer.Commands.EditPz;
using Rogbau.Api.Features.EquipmentTransfer.Commands.EditWz;
using Rogbau.Api.Features.EquipmentTransfer.Commands.EditTransfer;
using Rogbau.Api.Features.EquipmentTransfer.Commands.CreateTransfer;
using Rogbau.Api.Features.EquipmentTransfer.Commands.CreateWz;
using Rogbau.Api.Features.EquipmentTransfer.Dtos;
using Rogbau.Api.Features.EquipmentTransfer.Queries.GetAllTransfers;
using Rogbau.Api.Features.EquipmentTransfer.Queries.GetTransferById;

namespace Rogbau.Api.Features.EquipmentTransfer
{
    [Authorize]
    [ApiController]
    [Route("api/transfers")]
    public class EquipmentTransferController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EquipmentTransferController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin, Magazynier")]
        [HttpPost("createWz")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateWzCommand command, CancellationToken cancellationToken)
        {
            var wzId = await _mediator.Send(command);

            return Ok(wzId);
        }

        [Authorize(Roles = "Kierownik Budowy")]
        [HttpPost("createPz")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreatePzCommand command, CancellationToken cancellationToken)
        {
            var pzId = await _mediator.Send(command);

            return Ok(pzId);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/pz")]
        public async Task<IActionResult> EditPz(Guid id, [FromBody] EditPzCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/wz")]
        public async Task<IActionResult> EditWz(Guid id, [FromBody] EditWzCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/transfer")]
        public async Task<IActionResult> EditTransfer(Guid id, [FromBody] EditTransferCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<TransferDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
     [FromQuery] GetAllTransfersQuery query,
     CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "Admin, Kierownik Budowy, Magazynier")]
        [HttpPost("createTransfer")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTransfer(
    [FromBody] CreateTransferCommand command,
    CancellationToken cancellationToken)
        {
            var transferId = await _mediator.Send(command, cancellationToken);

            return Ok(transferId);
        }

        [Authorize(Roles = "Admin,Magazynier,Kierownik Budowy")]
        [HttpPatch("{id:guid}/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Confirm(
    Guid id,
    [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] ConfirmEquipmentMovementCommand? receipt,
    CancellationToken cancellationToken)
        {
            var command = new ConfirmEquipmentMovementCommand
            {
                Id = id,
                Items = receipt?.Items
            };

            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TransferDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTransferByIdQuery(id),
                cancellationToken);

            return Ok(result);
        }
    }
}

