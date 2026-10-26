using MediatR;
using Rogbau.Api.Features.Auth.Login.Dtos;

namespace Rogbau.Api.Features.Auth.Login.Commands
{
    public record LoginCommand(
        string Email,
        string Password
        ) : IRequest<LoginResponseDto>;
}
