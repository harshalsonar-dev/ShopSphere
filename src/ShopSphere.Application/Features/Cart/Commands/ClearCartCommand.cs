using MediatR;
using ShopSphere.Application.Common.Interfaces;

namespace ShopSphere.Application.Features.Cart.Commands;

public record ClearCartCommand(string CartKey) : IRequest<bool>;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly ICacheService _cacheService;

    public ClearCartCommandHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"cart:{request.CartKey}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        return true;
    }
}
