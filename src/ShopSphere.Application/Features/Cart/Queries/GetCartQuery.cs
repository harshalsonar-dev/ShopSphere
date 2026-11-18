using MediatR;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.DTOs;

namespace ShopSphere.Application.Features.Cart.Queries;

public record GetCartQuery(string CartKey) : IRequest<CartDto>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICacheService _cacheService;

    public GetCartQueryHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"cart:{request.CartKey}";
        var cart = await _cacheService.GetAsync<CartDto>(cacheKey, cancellationToken);

        if (cart == null)
        {
            cart = new CartDto { CartKey = request.CartKey };
        }

        return cart;
    }
}
