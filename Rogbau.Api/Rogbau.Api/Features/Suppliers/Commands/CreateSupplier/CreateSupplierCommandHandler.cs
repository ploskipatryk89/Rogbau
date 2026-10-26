using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Common.Exceptions.Equipments;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Suppliers.Commands.CreateSupplier
{
    public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreateSupplierCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.Name == request.Name);

            if (supplierExists)
            {
                throw new AlreadyExistsException($"Dostawca o nazwie {request.Name} już istnieje");
            }

            var newSupplier = new Supplier
            {
                Name = request.Name
            };

            _context.Suppliers.Add(newSupplier);

            await _context.SaveChangesAsync(cancellationToken);

            return newSupplier.Id;
        }
    }
}
