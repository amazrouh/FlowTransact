# Troubleshooting Guide

## Transactions API: UseBusOutbox Blocking (Fixed)

### Symptom (historical)
- Create/Submit requests would hang or timeout with `UseBusOutbox=true`
- Health check worked, but API calls to create transactions blocked

### Root Cause
Publishing domain events from the **SaveChanges interceptor** (during `SaveChanges`) caused a deadlock with MassTransit's bus outbox infrastructure. The outbox expects `Publish` to be called *before* `SaveChanges`, not during it.

### Fix (applied)
Domain event publishing was moved from the interceptor to the **repository**, calling `PublishDomainEventsAsync` *before* `SaveChangesAsync`. This matches MassTransit's expected flow: Publish → outbox adds message → SaveChanges commits both.

### If Issues Persist
Set `Messaging:UseBusOutbox=false` to bypass the outbox (Publish goes directly to RabbitMQ). Trade-off: you lose the transactional guarantee.

---

## TransactionSubmitted Not Reaching Payments

### Symptom
- `StartPayment` returns `alreadyExisted: false` after submitting a transaction
- Payment is not auto-created when transaction is submitted

### Root Cause
1. **Exchange binding**: Queue `transaction-submitted` must bind to `MoneyFellows.Contracts.Events:TransactionSubmitted`
2. **Outbox**: If UseBusOutbox is enabled and messages are stuck in outbox, they never reach RabbitMQ

### Fix
1. **Explicit bind** in Payments `MassTransitConfiguration.cs`:
   ```csharp
   e.Bind(TransactionSubmittedConstants.MessageUrn);
   ```
2. **UseBusOutbox=false** if startup hangs (see above)
3. Run `scripts/inspect-rabbitmq-topology.ps1` to verify bindings
