using MediatR;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Interfaces;

namespace ShopSphere.Application.Features.Orders.Commands;

public record CancelOrderCommand(int OrderId, int UserId, bool IsAdmin) : IRequest<bool>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        if (!request.IsAdmin && order.UserId != request.UserId)
        {
            throw new BadRequestException("You are not authorized to cancel this order.");
        }

        if (order.OrderStatus == OrderStatus.Delivered)
        {
            throw new BadRequestException("Delivered orders cannot be cancelled.");
        }

        if (order.OrderStatus == OrderStatus.Cancelled)
        {
            return true;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Restock products
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product != null)
                {
                    product.Restock(item.Quantity);
                    _productRepository.Update(product);
                }
            }

            order.Cancel();
            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
