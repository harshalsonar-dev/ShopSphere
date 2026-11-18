using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.Queries;
using ShopSphere.Application.Features.Orders.Commands;
using ShopSphere.Application.Features.Orders.Queries;
using ShopSphere.Domain.Entities;

namespace ShopSphere.Web.Controllers;

public class OrdersController : Controller
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    private string ResolveCartKey()
    {
        if (_currentUserService.UserId.HasValue)
        {
            return $"user_{_currentUserService.UserId.Value}";
        }

        if (Request.Cookies.TryGetValue("cart_session_id", out var sessionKey) && !string.IsNullOrWhiteSpace(sessionKey))
        {
            return sessionKey;
        }

        return "unknown";
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in or register to complete your order.";
            return RedirectToAction("Login", "Account", new { returnUrl = "/Orders/Checkout" });
        }

        var cartKey = ResolveCartKey();
        var cart = await _mediator.Send(new GetCartQuery(cartKey), cancellationToken);

        if (cart == null || cart.Items.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty. Add items before checking out.";
            return RedirectToAction("Index", "Cart");
        }

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCheckout(CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartKey = ResolveCartKey();
        try
        {
            var command = new CheckoutOrderCommand(_currentUserService.UserId.Value, cartKey);
            var order = await _mediator.Send(command, cancellationToken);

            TempData["SuccessMessage"] = $"Order #{order.Id} placed successfully! Real-time inventory has been deducted.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Checkout));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Orders" });
        }

        var orders = await _mediator.Send(new GetUserOrdersQuery(_currentUserService.UserId.Value), cancellationToken);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = _currentUserService.Role == Role.AdminRole;
        var userId = _currentUserService.UserId ?? 0;

        var order = await _mediator.Send(new GetOrderByIdQuery(id, userId, isAdmin), cancellationToken);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = _currentUserService.Role == Role.AdminRole;
        try
        {
            await _mediator.Send(new CancelOrderCommand(id, _currentUserService.UserId.Value, isAdmin), cancellationToken);
            TempData["SuccessMessage"] = $"Order #{id} has been cancelled and stock has been restocked.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
