using MediatR;
using Microsoft.Extensions.Logging;
using ShopSphere.Application.Common.Interfaces;

namespace ShopSphere.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cacheKey = cacheableQuery.CacheKey;

        // Try getting cached response
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for key: {CacheKey}. Fetching from database.", cacheKey);
        var response = await next();

        if (response != null)
        {
            var expiration = cacheableQuery.Expiration ?? TimeSpan.FromMinutes(5);
            await _cacheService.SetAsync(cacheKey, response, absoluteExpiration: expiration, cancellationToken: cancellationToken);
        }

        return response;
    }
}
