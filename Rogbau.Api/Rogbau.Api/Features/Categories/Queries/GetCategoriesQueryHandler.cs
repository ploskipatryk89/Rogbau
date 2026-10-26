using MediatR;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Features.Categories.Dtos;
using Rogbau.Api.Models.Data;

namespace Rogbau.Api.Features.Categories.Queries
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetCategoriesQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.SubCategories)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        CategoryId = sc.CategoryId
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}
