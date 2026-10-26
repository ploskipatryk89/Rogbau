using MediatR;
using Rogbau.Api.Features.Roles.Dtos;

namespace Rogbau.Api.Features.Roles.Queries
{
    public record GetAllRolesQuery : IRequest<List<RoleDto>>
    {
    }
}
