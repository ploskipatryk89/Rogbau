namespace Rogbau.Api.Features.Auth.CurrentUser.Dtos
{
    public record CurrentUserResponseDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string FullName,
        string Initials,
        string Role,
        bool MustChangePassword
    );
}
