using MediatR;
using Microsoft.AspNetCore.Identity;
using Rogbau.Api.Models;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Users.Commands.ChangePasswordByAdmin
{
    public class ChangePasswordByAdminCommandHandler : IRequestHandler<ChangePasswordByAdminCommand, Unit>
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
       

        public ChangePasswordByAdminCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
           
        }

        public async Task<Unit> Handle(ChangePasswordByAdminCommand request, CancellationToken cancellationToken)
        {
            var user =await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                throw new KeyNotFoundException($"Uzytkownik nie istnieje");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
       ", ",
       result.Errors.Select(e => e.Description)
   );

                throw new InvalidOperationException(errors);
            }

            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);
            return Unit.Value;
        }
    }
}
