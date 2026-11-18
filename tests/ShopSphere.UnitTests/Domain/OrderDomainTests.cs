using FluentAssertions;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Exceptions;
using Xunit;

namespace ShopSphere.UnitTests.Domain;

public class OrderDomainTests
{
    [Fact]
    public void AddItem_ShouldAddOrderItemAndRecalculateTotal()
    {
        // Arrange
        var order = new Order(1);

        // Act
        order.AddItem(productId: 10, quantity: 2, unitPrice: 50.00m);
        order.AddItem(productId: 20, quantity: 1, unitPrice: 100.00m);

        // Assert
        order.OrderItems.Should().HaveCount(2);
        order.TotalAmount.Should().Be(200.00m);
    }

    [Fact]
    public void AddItem_WhenItemAlreadyExists_ShouldIncrementQuantity()
    {
        // Arrange
        var order = new Order(1);

        // Act
        order.AddItem(productId: 10, quantity: 2, unitPrice: 50.00m);
        order.AddItem(productId: 10, quantity: 3, unitPrice: 50.00m);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(5);
        order.TotalAmount.Should().Be(250.00m);
    }

    [Fact]
    public void UpdateStatus_ShouldTransitionStatus()
    {
        // Arrange
        var order = new Order(1);

        // Act
        order.UpdateStatus(OrderStatus.Processing);

        // Assert
        order.OrderStatus.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Cancel_WhenDelivered_ShouldThrowDomainException()
    {
        // Arrange
        var order = new Order(1);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot cancel an order that has already been delivered*");
    }
}
