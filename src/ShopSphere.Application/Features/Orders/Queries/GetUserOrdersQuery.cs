using MediatR;
using ShopSphere.Application.Features.Orders.DTOs;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Orders.Queries;

public record GetUserOrdersQuery(int UserId) : IRequest<IReadOnlyList<OrderDto>>;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetUserOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(request.UserId, cancellationToken);

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            UserId = o.UserId,
            CustomerName = o.User?.FullName ?? string.Empty,
            CustomerEmail = o.User?.Email ?? string.Empty,
            TotalAmount = o.TotalAmount,
            OrderStatus = o.OrderStatus,
            CreatedAt = o.CreatedAt,
            Items = o.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? $"Product #{oi.ProductId}",
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity
            }).ToList()
        }).ToList();
    }
}
