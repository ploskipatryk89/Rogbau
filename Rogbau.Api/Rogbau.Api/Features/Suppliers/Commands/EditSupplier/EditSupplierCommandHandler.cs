using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions.Equipments;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Suppliers.Commands.EditSupplier;

public class EditSupplierCommandHandler(ApplicationDbContext context) : IRequestHandler<EditSupplierCommand, Unit>
{
    public async Task<Unit> Handle(EditSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Nie znaleziono dostawcy.");
        var name = request.Name.Trim();
        if (await context.Suppliers.AnyAsync(s => s.Id != request.Id && s.Name.Trim().ToLower() == name.ToLower(), cancellationToken))
            throw new AlreadyExistsException($"Dostawca o nazwie {name} już istnieje.");
        supplier.Name = name;
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
