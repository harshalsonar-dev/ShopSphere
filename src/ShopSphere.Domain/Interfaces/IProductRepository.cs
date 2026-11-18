using ShopSphere.Domain.Entities;

namespace ShopSphere.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredProductsAsync(
        string? searchTerm,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);
}
