using FluentAssertions;
using Moq;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.DTOs;
using ShopSphere.Application.Features.Orders.Commands;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;
using Xunit;

namespace ShopSphere.UnitTests.Features.Orders;

public class CheckoutOrderCommandHandlerTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CheckoutOrderCommandHandler _handler;

    public CheckoutOrderCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _productRepoMock = new Mock<IProductRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _userRepoMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CheckoutOrderCommandHandler(
            _cacheServiceMock.Object,
            _productRepoMock.Object,
            _orderRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCartIsEmpty_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com", "hash", 2);
        _userRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cacheServiceMock.Setup(c => c.GetAsync<CartDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CartDto { Items = new List<CartItemDto>() });

        var command = new CheckoutOrderCommand(1, "test_cart");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*shopping cart is empty*");
    }

    [Fact]
    public async Task Handle_WhenValidOrder_ShouldDeductStockAndCommitTransaction()
    {
        // Arrange
        var user = new User(1, "Jane Customer", "jane@example.com", "hash", 2);
        _userRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var product = new Product(5, "Ergonomic Keyboard", "RGB", 150.00m, 10, 1);
        _productRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var cart = new CartDto
        {
            CartKey = "user_1",
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = 5, ProductName = "Ergonomic Keyboard", UnitPrice = 150.00m, Quantity = 2 }
            }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<CartDto>("cart:user_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new CheckoutOrderCommand(1, "user_1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(300.00m);
        product.StockQuantity.Should().Be(8); // Real-time deduction: 10 - 2 = 8

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("cart:user_1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
