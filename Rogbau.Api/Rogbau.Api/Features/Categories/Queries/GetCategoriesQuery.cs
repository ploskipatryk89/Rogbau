using MediatR;
using Rogbau.Api.Features.Categories.Dtos;

namespace Rogbau.Api.Features.Categories.Queries
{
    public record GetCategoriesQuery : IRequest<List<CategoryDto>>;
}
