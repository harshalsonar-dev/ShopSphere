using MediatR;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Common.Models;
using ShopSphere.Application.Features.Products.DTOs;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Products.Queries;

public record GetProductsQuery(
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int PageIndex = 1,
    int PageSize = 10) : IRequest<PagedResult<ProductDto>>, ICacheableQuery
{
    public string CacheKey =>
        $"products:cat_{CategoryId?.ToString() ?? "all"}:p_{MinPrice?.ToString() ?? "0"}_{MaxPrice?.ToString() ?? "max"}:q_{SearchTerm?.ToLower() ?? "all"}:p_{PageIndex}_sz_{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetFilteredProductsAsync(
            request.SearchTerm,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        var dtoList = items.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            CreatedAt = p.CreatedAt
        }).ToList();

        return PagedResult<ProductDto>.Create(dtoList, totalCount, request.PageIndex, request.PageSize);
    }
}
