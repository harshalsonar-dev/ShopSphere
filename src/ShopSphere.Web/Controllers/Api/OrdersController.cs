using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Common.Models;
using ShopSphere.Application.Features.Orders.Commands;
using ShopSphere.Application.Features.Orders.DTOs;
using ShopSphere.Application.Features.Orders.Queries;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;

namespace ShopSphere.Web.Controllers.Api;

[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
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

    [Authorize]
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized(ApiResponse<object>.Fail("User must be authenticated to checkout."));
        }

        var cartKey = ResolveCartKey();
        var result = await _mediator.Send(new CheckoutOrderCommand(_currentUserService.UserId.Value, cartKey), cancellationToken);

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, ApiResponse<OrderDto>.Ok(result, "Order created successfully."));
    }

    [Authorize]
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized(ApiResponse<object>.Fail("User authentication required."));
        }

        var result = await _mediator.Send(new GetUserOrdersQuery(_currentUserService.UserId.Value), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.Ok(result, "User orders retrieved."));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUserService.Role == Role.AdminRole;
        var userId = _currentUserService.UserId ?? 0;

        var result = await _mediator.Send(new GetOrderByIdQuery(id, userId, isAdmin), cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Order with ID {id} was not found."));
        }

        return Ok(ApiResponse<OrderDto>.Ok(result, "Order retrieved successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.Ok(result, "All customer orders retrieved."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateOrderStatusCommand(id, request.Status), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Order status updated successfully."));
    }

    [Authorize]
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelOrder(int id, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUserService.Role == Role.AdminRole;
        var userId = _currentUserService.UserId ?? 0;

        var result = await _mediator.Send(new CancelOrderCommand(id, userId, isAdmin), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Order cancelled successfully and inventory restocked."));
    }
}

public class UpdateStatusRequest
{
    public OrderStatus Status { get; set; }
}
