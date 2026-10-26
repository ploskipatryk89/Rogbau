using MediatR;

namespace Rogbau.Api.Features.Suppliers.Commands.CreateSupplier
{
    public record CreateSupplierCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }
}
