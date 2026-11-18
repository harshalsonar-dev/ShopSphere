using ShopSphere.Domain.Common;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    // Navigation property
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    protected Category() { }

    public Category(string name)
    {
        UpdateName(name);
    }

    public Category(int id, string name)
    {
        Id = id;
        UpdateName(name);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
