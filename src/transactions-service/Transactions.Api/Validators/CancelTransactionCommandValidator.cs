using FluentValidation;
using Transactions.Application.Commands;

namespace Transactions.Api.Validators;

public class CancelTransactionCommandValidator : AbstractValidator<CancelTransactionCommand>
{
    public CancelTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required");
    }
}
