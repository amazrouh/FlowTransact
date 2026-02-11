using FluentValidation;
using Transactions.Application.Commands;

namespace Transactions.Api.Validators;

public class AddTransactionItemCommandValidator : AbstractValidator<AddTransactionItemCommand>
{
    public AddTransactionItemCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName is required")
            .Length(1, 200).WithMessage("ProductName must be between 1 and 200 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("UnitPrice must be greater than 0");
    }
}