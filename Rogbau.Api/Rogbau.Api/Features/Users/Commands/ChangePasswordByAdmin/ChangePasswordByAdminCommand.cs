using MediatR;

namespace Rogbau.Api.Features.Users.Commands.ChangePasswordByAdmin
{
    public record ChangePasswordByAdminCommand(string Id, string NewPassword) : IRequest<Unit>;
    
    
}
