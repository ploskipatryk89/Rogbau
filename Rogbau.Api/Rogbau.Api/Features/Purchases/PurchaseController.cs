using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Categories.Commands.DeleteSubCategory;
using Rogbau.Api.Features.Equipments.Commands.CreateEquipment;
using Rogbau.Api.Features.Purchases.Commands;
using Rogbau.Api.Features.Purchases.Commands.DeletePurchase;
using Rogbau.Api.Features.Purchases.Commands.EditPurchase;
using Rogbau.Api.Features.Purchases.Dtos;
using Rogbau.Api.Features.Purchases.Queries;

namespace Rogbau.Api.Features.Purchases
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchaseController : ControllerBase
    {

        private readonly IMediator _mediator;

        public PurchaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createPurchase")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseCommand command, CancellationToken cancellationToken)
        {
            var purchaseId = await _mediator.Send(command);

            return Ok(purchaseId);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditPurchaseCommand command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command with { Id = id }, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin,Magazynier")]
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<PurchaseListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginatedRequest pagination, CancellationToken cancellationToken)
        {
            var query = new GetAllPurchasesQuery()
            {
                Pagination = pagination
            };

            var result = await
                _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePurchaseCommand(id);

            try
            {
                await _mediator.Send(command);
                return NoContent();
            } catch(KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
