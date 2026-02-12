using MediatR;

namespace Payments.Application.Commands;

/// <param name="Amount">When provided (event path), used directly. When null (manual API), fetched from Transactions API with customerId validation.</param>
public record StartPaymentCommand(Guid TransactionId, Guid CustomerId, decimal? Amount = null) : IRequest<StartPaymentResult>;

public record StartPaymentResult(Guid PaymentId, bool AlreadyExisted);
