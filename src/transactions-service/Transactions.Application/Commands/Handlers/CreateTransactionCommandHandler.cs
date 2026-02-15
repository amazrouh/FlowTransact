using MediatR;
using Microsoft.Extensions.Logging;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(ITransactionRepository transactionRepository, ILogger<CreateTransactionCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction(request.CustomerId);
        await _transactionRepository.AddAsync(transaction, cancellationToken);
        _logger.LogInformation("Transaction created: {TransactionId}, CustomerId: {CustomerId}", transaction.Id, request.CustomerId);
        return transaction.Id;
    }
}