using MediatR;

namespace Rogbau.Api.Features.Suppliers.Commands.EditSupplier;

public record EditSupplierCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
