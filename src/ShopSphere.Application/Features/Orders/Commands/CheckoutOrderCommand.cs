using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Cart.DTOs;
using ShopSphere.Application.Features.Orders.DTOs;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Orders.Commands;

public record CheckoutOrderCommand(int UserId, string CartKey) : IRequest<OrderDto>;

public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, OrderDto>
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutOrderCommandHandler(
        ICacheService cacheService,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify User exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        // 2. Fetch Shopping Cart from Redis
        var cacheKey = $"cart:{request.CartKey}";
        var cart = await _cacheService.GetAsync<CartDto>(cacheKey, cancellationToken);

        if (cart == null || cart.Items.Count == 0)
        {
            throw new BadRequestException("Cannot checkout: Your shopping cart is empty.");
        }

        // 3. Execute Checkout within a resilient database transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = new Order(request.UserId);

            foreach (var cartItem in cart.Items)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
                if (product == null)
                {
                    throw new NotFoundException(nameof(Product), cartItem.ProductId);
                }

                // Rich Domain Model invariant: Deduct stock atomically and check threshold
                product.DeductStock(cartItem.Quantity);
                _productRepository.Update(product);

                // Add item to order
                order.AddItem(product.Id, cartItem.Quantity, product.Price);
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 4. Invalidate / clear cart in Redis upon successful checkout
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);

            // 5. Construct returned DTO
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = user.FullName,
                CustomerEmail = user.Email,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = cart.Items.FirstOrDefault(ci => ci.ProductId == oi.ProductId)?.ProductName ?? $"Product #{oi.ProductId}",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
