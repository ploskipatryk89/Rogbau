using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Auth.CurrentUser.Queries;
using Rogbau.Api.Features.Auth.Login.Commands;
using System.Security.Claims;

namespace Rogbau.Api.Features.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")] // <-- TEGO BRAKOWAŁO (Swagger tego wymaga)
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken); // <-- Dodany await

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(
                new GetCurrentUserQuery(userId)
            );

            return Ok(result);
        }
    }
}