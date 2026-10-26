using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Equipments.Commands;
using Rogbau.Api.Features.Equipments.Commands.DeleteEquipment;
using Rogbau.Api.Features.Equipments.Commands.EditEquipment;
using Rogbau.Api.Features.Equipments.Dtos;
using Rogbau.Api.Features.Users.Commands.ChangePasswordByAdmin;
using Rogbau.Api.Features.Users.Commands.ChangePasswordByUser;
using Rogbau.Api.Features.Users.Commands.CreateUser;
using Rogbau.Api.Features.Users.Commands.DeleteUser;
using Rogbau.Api.Features.Users.Commands.EditUser;
using Rogbau.Api.Features.Users.Dtos;
using Rogbau.Api.Features.Users.Queries.GetAllUsers;

namespace Rogbau.Api.Features.Users
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
        {
            var userId = await _mediator.Send(command);

            return Ok(userId);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllUsersQuery();

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(string id)
        {
            var command = new DeleteUserCommand(id);

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

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordByUserCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPut("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(
    string id,
    [FromBody] ChangePasswordByAdminCommand request)
        {
            var command = new ChangePasswordByAdminCommand(
                id,
                request.NewPassword
            );

            await _mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] EditUserCommand command)
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
    }
}
