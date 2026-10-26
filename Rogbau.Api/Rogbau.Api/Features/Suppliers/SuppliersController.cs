using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Features.Sites.Commands.CreateSite;
using Rogbau.Api.Features.Sites.Dtos;
using Rogbau.Api.Features.Suppliers.Commands.CreateSupplier;
using Rogbau.Api.Features.Suppliers.Commands.EditSupplier;
using Rogbau.Api.Features.Suppliers.Dtos;
using Rogbau.Api.Features.Suppliers.Queries.GetAllSuppliers;

namespace Rogbau.Api.Features.Suppliers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Magazynier")]
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditSupplierCommand command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command with { Id = id }, cancellationToken);
            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SupplierResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            // Tworzymy Query
            var query = new GetAllSuppliersQuery();

            // Wysyłamy do MediatR
            var result = await _mediator.Send(query);

            // Zwracamy HTTP 200 OK z wynikiem
            return Ok(result);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost("createSupplier")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command)
        {
            var supplierId = await _mediator.Send(command);

            // HTTP 201 Created z Location header
            return Ok(supplierId);
        }
    }
}
