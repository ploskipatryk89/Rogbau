using MediatR;
using Rogbau.Api.Features.Sites.Dtos;

namespace Rogbau.Api.Features.Sites.Queries.GetAllSites
{
    public record GetAllQuery : IRequest<List<SiteDto>>
    {
    }
}
