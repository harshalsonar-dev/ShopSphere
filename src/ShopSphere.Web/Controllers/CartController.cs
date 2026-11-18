using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.Commands;
using ShopSphere.Application.Features.Cart.Queries;

namespace ShopSphere.Web.Controllers;

public class CartController : Controller
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

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        var cart = await _mediator.Send(new GetCartQuery(cartKey), cancellationToken);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity, CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        try
        {
            await _mediator.Send(new UpdateCartItemCommand(cartKey, productId, quantity), cancellationToken);
            TempData["SuccessMessage"] = "Cart updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        await _mediator.Send(new RemoveFromCartCommand(cartKey, productId), cancellationToken);
        TempData["SuccessMessage"] = "Item removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        var cartKey = ResolveCartKey();
        await _mediator.Send(new ClearCartCommand(cartKey), cancellationToken);
        TempData["SuccessMessage"] = "Your cart has been cleared.";
        return RedirectToAction(nameof(Index));
    }
}
