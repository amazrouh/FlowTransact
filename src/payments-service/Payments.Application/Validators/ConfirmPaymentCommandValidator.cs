using FluentValidation;
using Payments.Application.Commands;

namespace Payments.Application.Validators;

public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("PaymentId is required");
    }
}
