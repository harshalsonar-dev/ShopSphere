using FluentValidation;
using ShopSphere.Application.Features.Orders.Commands;

namespace ShopSphere.Application.Features.Orders.Validators;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Valid order ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Valid order status must be specified.");
    }
}
