using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Sites.Commands.AssignSiteUser;
using Rogbau.Api.Features.Sites.Commands.CreateSite;
using Rogbau.Api.Features.Sites.Commands.DeleteUserFromSite;
using Rogbau.Api.Features.Sites.Commands.FinishSite;
using Rogbau.Api.Features.Sites.Commands.SetSiteInitialStock;
using Rogbau.Api.Features.Sites.Commands.StartSite;
using Rogbau.Api.Features.Sites.Dtos;
using Rogbau.Api.Features.Sites.Queries.GetAllSites;
using Rogbau.Api.Features.Sites.Queries.GetSiteById;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Rogbau.Api.Features.Sites
{
    [ApiController]
    [Route("api/[controller]")]
    public class SitesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _user;

        public SitesController(IMediator mediator, ApplicationDbContext context, ICurrentUserService user)
        {
            _mediator = mediator;
            _context = context;
            _user = user;
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMySites(CancellationToken cancellationToken)
        {
            var sites = _context.Sites.AsNoTracking().AsQueryable();
            if (!_user.IsInRole("Admin") && !_user.IsInRole("Magazynier"))
                sites = sites.Where(s => _context.SiteUsers.Any(x => x.SiteId == s.Id && x.UserId == _user.UserId));
            return Ok(await sites.Select(s => new { s.Id, s.Name, s.Status }).ToListAsync(cancellationToken));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createSite")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromBody] CreateSiteCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);

            return Ok(id);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<SiteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken)
        {
            var query = new GetAllQuery();

            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SiteDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetSiteByIdQuery(id),
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{siteId:guid}/users/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignUserToSite(
            Guid siteId,
            string userId,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new AssignSiteUserCommand(
                    siteId,
                    userId),
                cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{siteId:guid}/users/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUserFromSite(
            Guid siteId,
            string userId,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new DeleteUserFromSiteCommand(
                    userId,
                    siteId),
                cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = "Admin, Kierownik Budowy")]
        [HttpPost("{id:guid}/initial-stock")]
        public async Task<IActionResult> SetInitialStock(
            Guid id,
            [FromBody] SetSiteInitialStockCommand command)
        {
            if (command == null)
            {
                return BadRequest("Dane żądania są puste.");
            }

            command.SiteId = id;

            await _mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = "Admin, Kierownik Budowy")]
        [HttpPatch("{id:guid}/start")]
        public async Task<IActionResult> StartSite(
            Guid id,
            [FromBody] StartSiteCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(
                    new { message = "ID w URL różni się od ID w body" });
            }

            await _mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/finish")]
        public async Task<IActionResult> FinishSite(
            Guid id,
            [FromBody] FinishSiteCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(
                    new { message = "ID w URL różni się od ID w body" });
            }

            await _mediator.Send(command);

            return NoContent();
        }
    }
}
