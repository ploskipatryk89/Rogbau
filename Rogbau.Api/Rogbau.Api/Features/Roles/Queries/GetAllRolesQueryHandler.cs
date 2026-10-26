using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Roles.Dtos;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Roles.Queries
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
    {
        private RoleManager<IdentityRole> _roleManager;

        public GetAllRolesQueryHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }



        public async Task<List<RoleDto>> Handle(
    GetAllRolesQuery request,
    CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!
                })
                .ToListAsync(cancellationToken);

            return roles;
        }
    }
}
