using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Products.DTOs;

namespace ShopSphere.Application.Features.Products.Queries;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>, ICacheableQuery
{
    public string CacheKey => "categories:all";
    public TimeSpan? Expiration => TimeSpan.FromHours(1);
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);
    }
}
