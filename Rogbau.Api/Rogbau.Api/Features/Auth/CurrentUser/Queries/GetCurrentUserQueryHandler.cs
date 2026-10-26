using MediatR;
using Microsoft.AspNetCore.Identity;
using Rogbau.Api.Features.Auth.CurrentUser.Dtos;
using Rogbau.Api.Models;

namespace Rogbau.Api.Features.Auth.CurrentUser.Queries
{
    public class GetCurrentUserQueryHandler
         : IRequestHandler<GetCurrentUserQuery, CurrentUserResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCurrentUserQueryHandler(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CurrentUserResponseDto> Handle(
            GetCurrentUserQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "Nie znaleziono zalogowanego użytkownika");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var fullName =
                $"{user.FirstName} {user.LastName}".Trim();

            var initials = string.Concat(
                user.FirstName.FirstOrDefault(),
                user.LastName.FirstOrDefault()
            ).ToUpperInvariant();

            return new CurrentUserResponseDto(
                Id: user.Id ?? string.Empty,
                Email: user.Email ?? string.Empty,
                FirstName: user.FirstName,
                LastName: user.LastName,
                FullName: fullName,
                Initials: initials,
                Role: roles.FirstOrDefault() ?? string.Empty,
                MustChangePassword: user.MustChangePassword
            );
        }
    }
}
