namespace Rogbau.Api.Features.Auth.Login.Dtos
{
    public record LoginResponseDto(
        string Token,
        string Email,
        string FirstName,
        string LastName,
        bool MustChangePassword,
        string FullName,
        string Initials,
        string Role
    );
}