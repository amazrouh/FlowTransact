namespace Payments.Application.Exceptions;

public class DuplicatePaymentException : Exception
{
    public Guid TransactionId { get; }

    public DuplicatePaymentException(Guid transactionId)
        : base($"A payment already exists for transaction {transactionId}.")
    {
        TransactionId = transactionId;
    }
}
