# Debugging TransactionSubmitted (Publish not reaching Payments)

Use these steps when `TransactionSubmitted` is published but not received by the Payments consumer.

---

## 1. Inspect RabbitMQ Topology

**Prerequisites:** RabbitMQ Management UI at http://localhost:15672 (guest/guest)

### Exchanges to check

| Exchange | Expected | Purpose |
|----------|----------|---------|
| `MoneyFellows.Contracts.Events:TransactionSubmitted` | ✓ | Message-type exchange for Publish (after removing EntityName) |
| `transaction-submitted` | May exist from old config | Legacy if EntityName was used |

### Queue bindings

1. Go to **Queues** → `transaction-submitted`
2. Open **Bindings** tab
3. Confirm the queue is bound to `MoneyFellows.Contracts.Events:TransactionSubmitted`

**If the binding is missing or points to a different exchange**, the consumer will not receive messages.

---

## 2. Enable MassTransit Debug Logging

Already configured in `appsettings.Development.json`:

```json
"MassTransit": "Debug"
```

Look for log lines showing:
- Exchange name used when publishing
- Routing key (if any)
- Delivery confirmations or errors

---

## 3. Check Outbox Status

**Endpoint:** `GET /api/diagnostics/outbox`

Returns:
- `TotalPending`: Messages in outbox not yet delivered
- `Recent`: Last 10 outbox messages with `MessageType`, `DestinationAddress`
- `Hint`: Suggested next step

**If TotalPending > 0:** Messages are stuck. Check MassTransit logs for delivery errors. Try `Messaging:UseBusOutbox=false` to bypass outbox.

---

## 4. Test Without Outbox

**Config:** `appsettings.Development.json`

```json
"Messaging": {
  "UseBusOutbox": false
}
```

Restart Transactions API. Publish will go directly to RabbitMQ instead of the outbox.

- **If it works without outbox:** Issue is in outbox delivery (serialization, routing, or broker connection).
- **If it still fails:** Issue is in exchange/binding topology.

---

## 5. SQL: Manual Outbox Check

```sql
-- Pending messages
SELECT sequence_number, message_id, message_type, sent_time, destination_address
FROM outbox_message
ORDER BY sequence_number DESC
LIMIT 10;

-- Outbox state (delivery progress)
SELECT * FROM outbox_state ORDER BY created DESC LIMIT 5;
```

---

## Quick Reference

| Symptom | Action |
|---------|--------|
| Messages in outbox, not delivered | Check MassTransit logs; try `UseBusOutbox=false` |
| Outbox empty, consumer not receiving | Check RabbitMQ bindings; verify exchange name |
| Works with Send, fails with Publish | EntityName removed; verify consumer binds to message-type exchange |
