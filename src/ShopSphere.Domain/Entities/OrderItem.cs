using ShopSphere.Domain.Common;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    public decimal SubTotal => Quantity * UnitPrice;

    protected OrderItem() { }

    public OrderItem(int productId, int quantity, decimal unitPrice)
    {
        if (productId <= 0)
            throw new DomainException("Invalid Product ID for order item.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void SetOrder(int orderId)
    {
        OrderId = orderId;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
