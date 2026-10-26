using MediatR;
using Rogbau.Api.Features.Sites.Dtos;

namespace Rogbau.Api.Features.Sites.Queries.GetSiteById
{
    public record GetSiteByIdQuery(Guid Id) : IRequest<SiteDetailsDto>;
    
    
}
