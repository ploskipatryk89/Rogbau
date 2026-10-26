using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Users.Commands.EditUser
{
    public class EditUserCommandHandler : IRequestHandler<EditUserCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public EditUserCommandHandler(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Unit> Handle(
    EditUserCommand request,
    CancellationToken cancellationToken)
        {
            if (request.Role is not ("Admin" or "Magazynier" or "Kierownik Budowy") ||
                string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Podaj adres email i poprawną rolę użytkownika.");
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                throw new KeyNotFoundException("Nie znaleziono użytkownika");
            }

            if (request.Role != DbInitializer.AdminRole)
                await AdminAccountProtection.EnsureCanRemoveAdminAsync(_context, user.Id, cancellationToken);

            // Aktualizacja danych
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.UserName = request.Email;
            user.PhoneNumber = request.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new Exception(
                    string.Join(", ", updateResult.Errors.Select(x => x.Description)));
            }

            // Pobieramy aktualne role użytkownika
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Zmieniamy rolę tylko wtedy, gdy faktycznie jest inna
            if (!currentRoles.Contains(request.Role))
            {
                // Usuwamy dotychczasowe role
                if (currentRoles.Any())
                {
                    var removeResult =
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!removeResult.Succeeded)
                    {
                        throw new Exception(
                            string.Join(", ",
                                removeResult.Errors.Select(x => x.Description)));
                    }
                }

                // Dodajemy nową rolę
                var addRoleResult =
                    await _userManager.AddToRoleAsync(user, request.Role);

                if (!addRoleResult.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ",
                            addRoleResult.Errors.Select(x => x.Description)));
                }

                var stampResult = await _userManager.UpdateSecurityStampAsync(user);
                if (!stampResult.Succeeded)
                    throw new InvalidOperationException("Nie udało się unieważnić dotychczasowych sesji użytkownika.");
            }

            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
