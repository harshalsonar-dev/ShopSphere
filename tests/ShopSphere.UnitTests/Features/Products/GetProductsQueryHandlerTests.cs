using FluentAssertions;
using Moq;
using ShopSphere.Application.Features.Products.Queries;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;
using Xunit;

namespace ShopSphere.UnitTests.Features.Products;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _handler = new GetProductsQueryHandler(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("4K Monitor", "IPS Display", 299.99m, 10, 1),
            new Product("Gaming Headset", "Surround Sound", 79.99m, 25, 2)
        };

        _productRepoMock.Setup(r => r.GetFilteredProductsAsync(
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        var query = new GetProductsQuery(SearchTerm: "4K", PageIndex: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("4K Monitor");
        result.Items[0].Price.Should().Be(299.99m);
    }
}
