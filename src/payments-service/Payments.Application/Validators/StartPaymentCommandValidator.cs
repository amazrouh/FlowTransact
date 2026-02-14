using FluentValidation;
using Payments.Application.Commands;

namespace Payments.Application.Validators;

public class StartPaymentCommandValidator : AbstractValidator<StartPaymentCommand>
{
    public StartPaymentCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required");
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");
    }
}
