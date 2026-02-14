-- Outbox diagnostic queries for TransactionSubmitted debugging
-- Run against FlowTransact.Transactions database

-- Pending messages (if count > 0, messages are stuck - not delivered to RabbitMQ)
SELECT COUNT(*) AS pending_count FROM outbox_message;

-- Recent outbox messages
SELECT sequence_number, message_id, message_type, sent_time, destination_address
FROM outbox_message
ORDER BY sequence_number DESC
LIMIT 10;

-- Outbox state (delivery progress)
SELECT outbox_id, created, delivered, last_sequence_number
FROM outbox_state
ORDER BY created DESC
LIMIT 5;
