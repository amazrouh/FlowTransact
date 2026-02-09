# MoneyFellows.Contracts

This library contains the shared contracts and domain events used across the FlowTransact microservices.

## Domain Events

### TransactionSubmitted
Published by the Transactions service when a transaction is submitted for payment processing.

### PaymentConfirmed
Published by the Payments service when a payment is successfully completed.

### PaymentFailed
Published by the Payments service when a payment fails.

## Usage

Reference this library in your services to ensure consistent event contracts:

```csharp
using MoneyFellows.Contracts.Events;

// In your service code
var @event = new TransactionSubmitted(
    transactionId: transaction.Id,
    customerId: transaction.CustomerId,
    totalAmount: transaction.TotalAmount);
```