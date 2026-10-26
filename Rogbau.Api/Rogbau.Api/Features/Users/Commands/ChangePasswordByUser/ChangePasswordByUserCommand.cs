using MediatR;

namespace Rogbau.Api.Features.Users.Commands.ChangePasswordByUser
{
    public record ChangePasswordByUserCommand(string CurrentPassword, string NewPassword) : IRequest<Unit>;
    
    
}
