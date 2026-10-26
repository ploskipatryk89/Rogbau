using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Users.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Users.Queries.GetAllUsers
{
    public class GetAllUserQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAllUserQueryHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<UserDto>> Handle(
    GetAllUsersQuery request,
    CancellationToken cancellationToken)
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? string.Empty
                });
            }

            return result;
        }
    }
}
