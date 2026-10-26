using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Services;

namespace Rogbau.Api.Features.Users.Commands.DeleteUser
{
    
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);

           
            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                throw new KeyNotFoundException("Uzytkownik nie istnieje");
            }

            if (currentUserId == request.Id)
            {
                throw new InvalidOperationException("Nie mozesz usunac wlasnego konta");
            }

            await AdminAccountProtection.EnsureCanRemoveAdminAsync(_context, user.Id, cancellationToken);
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
