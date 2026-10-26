using MediatR;
using Microsoft.AspNetCore.Identity;
using Rogbau.Api.Features.Auth.Login.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Auth.Login.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginCommandHandler(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponseDto> Handle(
    LoginCommand request,
    CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "Nieprawidłowy adres email lub hasło");
            }

            var isPasswordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    request.Password);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException(
                    "Nieprawidłowy adres email lub hasło");
            }

            var token =
                await _jwtTokenService.GenerateTokenAsync(user);

            var roles =
                await _userManager.GetRolesAsync(user);

            var fullName =
                $"{user.FirstName} {user.LastName}".Trim();

            var initials = string.Concat(
                user.FirstName.FirstOrDefault(),
                user.LastName.FirstOrDefault()
            ).ToUpperInvariant();

            return new LoginResponseDto(
                Token: token,
                Email: user.Email ?? string.Empty,
                FirstName: user.FirstName,
                LastName: user.LastName,
                MustChangePassword: user.MustChangePassword,
                FullName: fullName,
                Initials: initials,
                Role: roles.FirstOrDefault() ?? string.Empty
            );
        }
    }
}
