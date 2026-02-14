using FluentValidation;
using Transactions.Application.Commands;

namespace Transactions.Application.Validators;

public class SubmitTransactionCommandValidator : AbstractValidator<SubmitTransactionCommand>
{
    public SubmitTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required");
    }
}
