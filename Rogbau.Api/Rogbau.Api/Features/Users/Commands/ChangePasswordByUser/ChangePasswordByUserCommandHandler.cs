using MediatR;
using Microsoft.AspNetCore.Identity;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Users.Commands.ChangePasswordByUser
{
    public class ChangePasswordByUserCommandHandler : IRequestHandler<ChangePasswordByUserCommand, Unit>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordByUserCommandHandler(ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(ChangePasswordByUserCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var user =await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("Uzytkownik nie istnieje");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description)
                );

                throw new InvalidOperationException(errors);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
            return Unit.Value;

        }
    }
}
