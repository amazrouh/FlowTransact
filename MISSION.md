Your Mission: “Event-Driven Financial Transactions Platform”

🧠 Business Context

MoneyFellows systems deal with financial transactions, where:

    Services must be independently deployable

    Data consistency must be handled safely

    Events must never be lost or duplicated

    The system must be extensible for future financial modules

In this quest, you’ll design and implement a mini event-driven backend platform that reflects real-world fintech challenges.
📌 The Challenge
1️⃣ Step 1 — Domain Modeling (DDD)

Model two bounded contexts:
A) Transactions Context

    Create Transaction

    Add Transaction Items

    Submit Transaction

    Cancel Transaction (with clear business rules)

B) Payments Context

    Start Payment for a Transaction

    Confirm Payment

    Fail Payment

Requirements

    Clear Aggregates and invariants

    Domain Events such as:

        TransactionSubmitted

        PaymentConfirmed

        PaymentFailed

    Clean separation between bounded contexts

📄 Deliverable: DOMAIN.md explaining your domain decisions.
2️⃣ Step 2 — Microservices + Clean Architecture

Implement two .NET services:
Transactions Service

    Endpoints:

        POST /transactions

        POST /transactions/{id}/items

        POST /transactions/{id}/submit

        POST /transactions/{id}/cancel

Payments Service

    Endpoints:

        POST /payments/start

        POST /payments/{id}/confirm

        POST /payments/{id}/fail

Architecture requirements

    Clean Architecture layers (Domain / Application / Infrastructure / API)

    No infrastructure dependencies leaking into domain

    Explicit application use-cases

3️⃣ Step 3 — Event-Driven Workflow

Use a message broker (RabbitMQ preferred):

    TransactionSubmitted → published by Transactions service

    Payments service consumes event → creates payment intent

    Payment result publishes:

        PaymentConfirmed

        PaymentFailed

    Transactions service consumes events → updates transaction state

Reliability is key
✅ Implement Outbox Pattern (or equivalent)
✅ Idempotent event consumers
✅ Basic retry & dead-letter handling
4️⃣ Step 4 — Observability & Testing

Add:

    Correlation IDs across HTTP & events

    Structured logging

    Health checks

    Tests:

        Domain invariant tests

        Integration test covering the full happy path

🧱 Suggested Tech Stack

    .NET 8 / ASP.NET Core

    PostgreSQL or SQL Server

    RabbitMQ

    Docker Compose

    Optional frameworks: MassTransit, Wolverine, or NServiceBus (justify your choice)

🧩 Example Flow

1️⃣ Create transaction
2️⃣ Add items
3️⃣ Submit transaction → event published
4️⃣ Payment started
5️⃣ Payment confirmed
6️⃣ Transaction marked as completed