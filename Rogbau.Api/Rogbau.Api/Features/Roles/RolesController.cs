using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Roles.Dtos;
using Rogbau.Api.Features.Roles.Queries;
using Rogbau.Api.Features.Users.Dtos;
using Rogbau.Api.Features.Users.Queries.GetAllUsers;

namespace Rogbau.Api.Features.Roles
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllRolesQuery();

            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
