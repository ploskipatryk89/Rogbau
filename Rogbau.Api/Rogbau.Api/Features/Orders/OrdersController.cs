using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Orders.Commands.CreateOrder;
using Rogbau.Api.Features.Purchases.Commands;
using Rogbau.Api.Features.Orders.Queries;

namespace Rogbau.Api.Features.Orders
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Kierownik Budowy")]
     
        [HttpPost("createOrder")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var OrderId = await _mediator.Send(command, cancellationToken);

            return Ok(OrderId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new GetOrdersQuery(), cancellationToken));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var orders = await _mediator.Send(new GetOrdersQuery(id), cancellationToken);
            if (orders.Count == 0) return NotFound();
            return Ok(orders[0]);
        }
    }
}
