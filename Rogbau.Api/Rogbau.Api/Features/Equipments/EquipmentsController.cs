using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Equipments.Commands;
using Rogbau.Api.Features.Equipments.Commands.DeleteEquipment;
using Rogbau.Api.Features.Equipments.Commands.EditEquipment;

using Rogbau.Api.Features.Equipments.Dtos;
using Rogbau.Api.Features.Equipments.Queries.GetAllEquipments;
using Rogbau.Api.Features.Equipments.Queries.GetEquipmentById;
using Rogbau.Api.Features.Sites.Dtos;
using Rogbau.Api.Features.Sites.Queries.GetSiteById;

namespace Rogbau.Api.Features.Equipments
{
    [Authorize]
    [ApiController]
    [Route("api/equipments")]
    public class EquipmentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EquipmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createEquipment")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateEquipmentCommand command, CancellationToken cancellationToken)
        {
            var equipmentId = await _mediator.Send(command);

            return Ok(equipmentId);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginatedList<EquipmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginatedRequest pagination,
            [FromQuery] string? searchQuery,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? subCategoryId,
            CancellationToken cancellationToken)
        {
            var query = new GetAllEquipmentsQuery(
                searchQuery, isActive, categoryId, subCategoryId)
            {
                Pagination = pagination
            }; var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteEquipmentCommand(id);

            try
            {
                await _mediator.Send(command);
                return NoContent();

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] EditEquipmentCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "ID w URL różni się od ID w body" });
            }

            try
            {
                await _mediator.Send(command);
                return NoContent();  // HTTP 204 - sukces bez body
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EquipmentDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetEquipmentByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = $"Produkt o ID {id} nie został znaleziony" });
            }

            return Ok(result);
        }

       


    }
}
