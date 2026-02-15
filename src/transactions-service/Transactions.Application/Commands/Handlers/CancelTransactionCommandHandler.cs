using MediatR;
using Microsoft.Extensions.Logging;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<CancelTransactionCommandHandler> _logger;

    public CancelTransactionCommandHandler(ITransactionRepository transactionRepository, ILogger<CancelTransactionCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction with ID {request.TransactionId} not found");
        }

        transaction.Cancel();
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        _logger.LogInformation("Transaction cancelled: {TransactionId}", request.TransactionId);
    }
}
