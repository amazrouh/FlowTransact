using FluentValidation;
using Payments.Application.Commands;

namespace Payments.Application.Validators;

public class FailPaymentCommandValidator : AbstractValidator<FailPaymentCommand>
{
    public FailPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("PaymentId is required");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required");
    }
}
