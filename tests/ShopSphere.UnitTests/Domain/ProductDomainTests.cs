using FluentAssertions;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Exceptions;
using Xunit;

namespace ShopSphere.UnitTests.Domain;

public class ProductDomainTests
{
    [Fact]
    public void DeductStock_WithSufficientStock_ShouldReduceQuantity()
    {
        // Arrange
        var product = new Product("Mechanical Keyboard", "RGB Gaming", 99.99m, 20, 1);

        // Act
        product.DeductStock(5);

        // Assert
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void DeductStock_ExceedingAvailableStock_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("Mechanical Keyboard", "RGB Gaming", 99.99m, 10, 1);

        // Act
        var act = () => product.DeductStock(15);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public void Restock_WithValidQuantity_ShouldIncreaseQuantity()
    {
        // Arrange
        var product = new Product("Mechanical Keyboard", "RGB Gaming", 99.99m, 10, 1);

        // Act
        product.Restock(25);

        // Assert
        product.StockQuantity.Should().Be(35);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var product = new Product("Old Name", "Old Desc", 50.00m, 10, 1);

        // Act
        product.UpdateDetails("New Name", "New Desc", 75.00m, 2);

        // Assert
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New Desc");
        product.Price.Should().Be(75.00m);
        product.CategoryId.Should().Be(2);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("Old Name", "Old Desc", 50.00m, 10, 1);

        // Act
        var act = () => product.UpdateDetails("", "Desc", 50.00m, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Product name cannot be empty*");
    }
}
