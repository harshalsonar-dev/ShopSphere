using FluentAssertions;
using Moq;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.Commands;
using ShopSphere.Application.Features.Cart.DTOs;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;
using Xunit;

namespace ShopSphere.UnitTests.Features.Cart;

public class AddToCartCommandHandlerTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly AddToCartCommandHandler _handler;

    public AddToCartCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _productRepoMock = new Mock<IProductRepository>();

        _handler = new AddToCartCommandHandler(_cacheServiceMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenStockIsInsufficient_ShouldThrowBadRequestException()
    {
        // Arrange
        var product = new Product("Gaming Mouse", "High DPI", 59.99m, 3, 1);
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new AddToCartCommand("cart_123", 1, 5); // requests 5 but only 3 available

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*exceeds available stock*");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldAddItemAndSaveToCache()
    {
        // Arrange
        var product = new Product("Gaming Mouse", "High DPI", 59.99m, 20, 1);
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _cacheServiceMock.Setup(c => c.GetAsync<CartDto>("cart:cart_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CartDto { CartKey = "cart_123" });

        var command = new AddToCartCommand("cart_123", 1, 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(2);
        result.TotalAmount.Should().Be(119.98m);

        _cacheServiceMock.Verify(c => c.SetAsync(
            "cart:cart_123",
            It.IsAny<CartDto>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
