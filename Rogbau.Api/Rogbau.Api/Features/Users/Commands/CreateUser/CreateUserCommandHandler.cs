using MediatR;
using Microsoft.AspNetCore.Identity;
using Rogbau.Api.Common.Exceptions.Equipments;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CreateUserCommandHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        public async Task<string> Handle(
     CreateUserCommand request,
     CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new AlreadyExistsException(
                    $"Użytkownik z adresem email {request.Email} już istnieje");
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MustChangePassword = true,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.TelephoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    string.Join(", ", roleResult.Errors.Select(x => x.Description)));
            }

            return user.Id;
        }
    }
}
