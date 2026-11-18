using MediatR;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.DTOs;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Cart.Commands;

public record AddToCartCommand(string CartKey, int ProductId, int Quantity) : IRequest<CartDto>;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;

    public AddToCartCommandHandler(ICacheService cacheService, IProductRepository productRepository)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        if (product.StockQuantity < request.Quantity)
        {
            throw new BadRequestException($"Requested quantity ({request.Quantity}) exceeds available stock ({product.StockQuantity}) for '{product.Name}'.");
        }

        var cacheKey = $"cart:{request.CartKey}";
        var cart = await _cacheService.GetAsync<CartDto>(cacheKey, cancellationToken) 
                   ?? new CartDto { CartKey = request.CartKey };

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem != null)
        {
            var newTotalQuantity = existingItem.Quantity + request.Quantity;
            if (product.StockQuantity < newTotalQuantity)
            {
                throw new BadRequestException($"Cannot add {request.Quantity} more. Total requested would be {newTotalQuantity}, but available stock is {product.StockQuantity}.");
            }
            existingItem.Quantity = newTotalQuantity;
        }
        else
        {
            cart.Items.Add(new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = request.Quantity
            });
        }

        // Cache cart for 7 days
        await _cacheService.SetAsync(cacheKey, cart, slidingExpiration: TimeSpan.FromDays(7), cancellationToken: cancellationToken);

        return cart;
    }
}
