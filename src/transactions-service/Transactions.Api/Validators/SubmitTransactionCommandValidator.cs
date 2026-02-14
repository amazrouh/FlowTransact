using FluentValidation;
using Transactions.Application.Commands;

namespace Transactions.Api.Validators;

public class SubmitTransactionCommandValidator : AbstractValidator<SubmitTransactionCommand>
{
    public SubmitTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required");
    }
}
