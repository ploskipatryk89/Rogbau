using MediatR;

namespace Rogbau.Api.Features.Users.Commands.CreateUser
{
    public record CreateUserCommand : IRequest<string>
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string TelephoneNumber { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
