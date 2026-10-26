using MediatR;

namespace Rogbau.Api.Features.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(string Id) : IRequest<Unit>;
   
   
}
