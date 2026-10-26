using MediatR;
using Rogbau.Api.Features.Users.Dtos;

namespace Rogbau.Api.Features.Users.Queries.GetAllUsers
{
    public record GetAllUsersQuery() : IRequest<List<UserDto>>;
    
    
}
