using MediatR;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.DTOs;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Cart.Commands;

public record UpdateCartItemCommand(string CartKey, int ProductId, int Quantity) : IRequest<CartDto>;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;

    public UpdateCartItemCommandHandler(ICacheService cacheService, IProductRepository productRepository)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
    }

    public async Task<CartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"cart:{request.CartKey}";
        var cart = await _cacheService.GetAsync<CartDto>(cacheKey, cancellationToken);
        if (cart == null)
        {
            throw new NotFoundException("Cart", request.CartKey);
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item == null)
        {
            throw new NotFoundException("CartItem", request.ProductId);
        }

        if (request.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product != null && product.StockQuantity < request.Quantity)
            {
                throw new BadRequestException($"Requested quantity ({request.Quantity}) exceeds stock ({product.StockQuantity}).");
            }
            item.Quantity = request.Quantity;
        }

        await _cacheService.SetAsync(cacheKey, cart, slidingExpiration: TimeSpan.FromDays(7), cancellationToken: cancellationToken);
        return cart;
    }
}
