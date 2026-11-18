using ShopSphere.Domain.Common;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Domain.Entities;

public class Order : BaseEntity
{
    public int UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Pending;

    // Navigation properties
    public User User { get; private set; } = null!;
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    protected Order() { }

    public Order(int userId)
    {
        if (userId <= 0)
            throw new DomainException("Valid user identifier is required for order.");

        UserId = userId;
        OrderStatus = OrderStatus.Pending;
        TotalAmount = 0m;
    }

    public OrderItem AddItem(int productId, int quantity, decimal unitPrice)
    {
        if (OrderStatus != OrderStatus.Pending)
            throw new DomainException("Cannot modify items on an order that is not Pending.");

        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            RecalculateTotal();
            return existingItem;
        }

        var newItem = new OrderItem(productId, quantity, unitPrice);
        _orderItems.Add(newItem);
        RecalculateTotal();
        return newItem;
    }

    public void RecalculateTotal()
    {
        TotalAmount = _orderItems.Sum(i => i.SubTotal);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (OrderStatus == OrderStatus.Cancelled && newStatus != OrderStatus.Cancelled)
            throw new DomainException("Cannot update status of a cancelled order.");

        if (OrderStatus == OrderStatus.Delivered && newStatus != OrderStatus.Delivered)
            throw new DomainException("Cannot change status once an order is delivered.");

        OrderStatus = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (OrderStatus == OrderStatus.Delivered)
            throw new DomainException("Cannot cancel an order that has already been delivered.");

        OrderStatus = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
