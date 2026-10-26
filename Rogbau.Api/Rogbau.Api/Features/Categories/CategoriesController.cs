using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rogbau.Api.Features.Categories.Commands.CreateCategory;
using Rogbau.Api.Features.Categories.Commands.EditCategory;
using Rogbau.Api.Features.Categories.Commands.DeleteCategory;
using Rogbau.Api.Features.Categories.Commands.CreateSubCategory;
using Rogbau.Api.Features.Categories.Commands.EditSubCategory;
using Rogbau.Api.Features.Categories.Commands.DeleteSubCategory;
using Rogbau.Api.Features.Categories.Dtos;
using Rogbau.Api.Features.Categories.Queries;

namespace Rogbau.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost()]
        public async Task<IActionResult> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(command, cancellationToken));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditCategory(Guid id, EditCategoryCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("subcategories")]
        public async Task<IActionResult> CreateSubCategory(CreateSubCategoryCommand command, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(command, cancellationToken));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("subcategories/{id:guid}")]
        public async Task<IActionResult> EditSubCategory(Guid id, EditSubCategoryCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("subcategories/{id:guid}")]
        public async Task<IActionResult> DeleteSubCategory(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteSubCategoryCommand { Id = id }, cancellationToken);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
        {
            var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
            return Ok(categories);
        }
    }
}
