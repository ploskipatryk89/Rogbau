using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.Equipments.Dtos;
using Rogbau.Api.Features.HistoricalData.Dtos;

namespace Rogbau.Api.Features.HistoricalData
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/historical-data")]
    public class HistoricalDataController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HistoricalDataController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<HistoricalDataDto>), StatusCodes.Status200OK)]

        public async Task<IActionResult> GetAll(
            [FromQuery] PaginatedRequest pagination,
            [FromQuery] GetAllHistoricalDataQuery query,
            CancellationToken cancellationToken)
        {
            query.Pagination = pagination;
            var result = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(result);
        }
    }
}
