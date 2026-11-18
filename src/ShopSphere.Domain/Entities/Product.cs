using ShopSphere.Domain.Common;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int CategoryId { get; private set; }

    // Navigation property
    public Category Category { get; private set; } = null!;

    protected Product() { }

    public Product(string name, string description, decimal price, int stockQuantity, int categoryId)
    {
        UpdateDetails(name, description, price, categoryId);
        SetStock(stockQuantity);
    }

    public Product(int id, string name, string description, decimal price, int stockQuantity, int categoryId)
        : this(name, description, price, stockQuantity, categoryId)
    {
        Id = id;
    }

    public void UpdateDetails(string name, string description, decimal price, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");

        if (price < 0)
            throw new DomainException("Product price cannot be negative.");

        if (categoryId <= 0)
            throw new DomainException("A valid Category must be specified.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Deduction quantity must be greater than zero.");

        if (StockQuantity < quantity)
            throw new DomainException($"Insufficient stock for product '{Name}'. Available: {StockQuantity}, Requested: {quantity}.");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Restock quantity must be greater than zero.");

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStock(int stockQuantity)
    {
        if (stockQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");

        StockQuantity = stockQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsInStock() => StockQuantity > 0;
}
