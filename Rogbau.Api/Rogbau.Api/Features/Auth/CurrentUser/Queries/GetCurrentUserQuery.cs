using MediatR;
using Rogbau.Api.Features.Auth.CurrentUser.Dtos;

namespace Rogbau.Api.Features.Auth.CurrentUser.Queries
{
    public record GetCurrentUserQuery(string UserId)
       : IRequest<CurrentUserResponseDto>;
}
