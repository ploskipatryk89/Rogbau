using MediatR;

namespace Rogbau.Api.Features.Users.Commands.EditUser
{
    public record EditUserCommand : IRequest<Unit>
    {
        public string Id { get; init; } = string.Empty;
        public string? FirstName { get; init; } = string.Empty;
        public string? LastName { get; init; } = string.Empty;
        public string? Email { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; } = string.Empty;
        public string? Role { get; init; } = string.Empty;


    }
    
    
}
