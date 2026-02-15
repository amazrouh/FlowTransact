# Troubleshooting Guide

## Transactions API: UseBusOutbox Blocking (Fixed)

### Symptom (historical)
- Create/Submit requests would hang or timeout with `UseBusOutbox=true`
- Health check worked, but API calls to create transactions blocked

### Root Cause
Publishing domain events from the **SaveChanges interceptor** (during `SaveChanges`) caused a deadlock with MassTransit's bus outbox infrastructure. The outbox expects `Publish` to be called *before* `SaveChanges`, not during it.

### Fix (applied)
Domain event publishing was moved from the interceptor to the **DbContext `SaveChangesAsync` override**, which calls `PublishDomainEventsAsync` *before* `base.SaveChangesAsync()`. This matches MassTransit's expected flow: Publish → outbox adds message → SaveChanges commits both.

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

---

## Seq Container Starts Then Stops

### Symptom
- Seq container starts but exits shortly after
- `docker ps` shows seq container is not running

### Fix (applied)
- Added `SEQ_FIRSTRUN_NOAUTHENTICATION: "true"` for dev (no auth on first run)
- Added `restart: unless-stopped` so the container retries

### If Issues Persist
1. **Check logs:** `docker logs flowtransact-seq-1` (or your seq container name)
2. **Clear corrupted volume:** If a previous run left bad state:
   ```bash
   docker-compose down
   docker volume rm flowtransact_seq_data
   docker-compose up -d seq
   ```
3. **Memory:** Seq may need more memory. Run with explicit limit:
   ```bash
   docker run -d --name seq -e ACCEPT_EULA=Y -e SEQ_FIRSTRUN_NOAUTHENTICATION=true -p 5341:80 -v flowtransact_seq_data:/data --memory=2g --memory-swap=2g datalust/seq:latest
   ```
