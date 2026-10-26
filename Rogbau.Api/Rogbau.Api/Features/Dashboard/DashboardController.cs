using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Dashboard.Dtos;
using Rogbau.Api.Features.Dashboard.Queries.GetDashboard;

namespace Rogbau.Api.Features.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin,Magazynier,Kierownik Budowy")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? siteId, CancellationToken cancellationToken)
        {
            var query = new GetDashboardQuery { SiteId = siteId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
