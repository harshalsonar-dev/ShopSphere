using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Common.Models;
using ShopSphere.Application.Features.Cart.Commands;
using ShopSphere.Application.Features.Cart.DTOs;
using ShopSphere.Application.Features.Cart.Queries;

namespace ShopSphere.Web.Controllers.Api;

[ApiController]
[Route("api/v1/cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CartController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    private string ResolveCartKey()
    {
        if (_currentUserService.IsAuthenticated && _currentUserService.UserId.HasValue)
        {
            return $"user_{_currentUserService.UserId.Value}";
        }

        // Anonymous session cart key stored in cookie or header
        if (Request.Cookies.TryGetValue("cart_session_id", out var sessionKey) && !string.IsNullOrWhiteSpace(sessionKey))
        {
            return sessionKey;
        }

        var newSessionKey = $"guest_{Guid.NewGuid():N}";
        Response.Cookies.Append("cart_session_id", newSessionKey, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return newSessionKey;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        var result = await _mediator.Send(new GetCartQuery(cartKey), cancellationToken);
        return Ok(ApiResponse<CartDto>.Ok(result, "Cart retrieved successfully."));
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        var result = await _mediator.Send(new AddToCartCommand(cartKey, request.ProductId, request.Quantity), cancellationToken);
        return Ok(ApiResponse<CartDto>.Ok(result, "Item added to cart."));
    }

    [HttpPut("items")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        var result = await _mediator.Send(new UpdateCartItemCommand(cartKey, request.ProductId, request.Quantity), cancellationToken);
        return Ok(ApiResponse<CartDto>.Ok(result, "Cart updated."));
    }

    [HttpDelete("items/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        var result = await _mediator.Send(new RemoveFromCartCommand(cartKey, productId), cancellationToken);
        return Ok(ApiResponse<CartDto>.Ok(result, "Item removed from cart."));
    }

    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        var result = await _mediator.Send(new ClearCartCommand(cartKey), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Cart cleared."));
    }
}

public class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
