using MediatR;

namespace Rogbau.Api.Features.Purchases.Commands.DeletePurchase
{
    public record DeletePurchaseCommand(Guid Id) : IRequest<Unit>;
    
    
}
