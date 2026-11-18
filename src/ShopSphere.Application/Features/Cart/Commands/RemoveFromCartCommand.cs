using MediatR;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.DTOs;

namespace ShopSphere.Application.Features.Cart.Commands;

public record RemoveFromCartCommand(string CartKey, int ProductId) : IRequest<CartDto>;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, CartDto>
{
    private readonly ICacheService _cacheService;

    public RemoveFromCartCommandHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<CartDto> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"cart:{request.CartKey}";
        var cart = await _cacheService.GetAsync<CartDto>(cacheKey, cancellationToken) 
                   ?? new CartDto { CartKey = request.CartKey };

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item != null)
        {
            cart.Items.Remove(item);
            await _cacheService.SetAsync(cacheKey, cart, slidingExpiration: TimeSpan.FromDays(7), cancellationToken: cancellationToken);
        }

        return cart;
    }
}
