using FluentValidation;
using ShopSphere.Application.Features.Orders.Commands;

namespace ShopSphere.Application.Features.Orders.Validators;

public class CheckoutOrderCommandValidator : AbstractValidator<CheckoutOrderCommand>
{
    public CheckoutOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user identifier is required.");

        RuleFor(x => x.CartKey)
            .NotEmpty().WithMessage("Cart key is required.");
    }
}
