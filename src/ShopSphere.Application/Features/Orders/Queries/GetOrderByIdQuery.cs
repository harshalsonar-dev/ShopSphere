using MediatR;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Features.Orders.DTOs;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Orders.Queries;

public record GetOrderByIdQuery(int OrderId, int UserId, bool IsAdmin) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId, cancellationToken);
        if (order == null)
            return null;

        if (!request.IsAdmin && order.UserId != request.UserId)
        {
            throw new BadRequestException("Unauthorized access to requested order.");
        }

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CustomerName = order.User?.FullName ?? string.Empty,
            CustomerEmail = order.User?.Email ?? string.Empty,
            TotalAmount = order.TotalAmount,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? $"Product #{oi.ProductId}",
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity
            }).ToList()
        };
    }
}
