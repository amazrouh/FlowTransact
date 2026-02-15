# Architecture: FlowTransact

This document describes the system architecture, design decisions, and implementation patterns for the FlowTransact event-driven financial transactions platform.

---

## 1. High-Level Architecture

FlowTransact is a **microservices platform** with two independently deployable services:

| Service | Responsibility | Database |
|---------|----------------|----------|
| **Transactions** | Transaction lifecycle (create, add items, submit, cancel) | FlowTransact.Transactions |
| **Payments** | Payment processing (start, confirm, fail) | FlowTransact.Payments |

Services communicate **asynchronously** via domain events over RabbitMQ. There are no direct database relationships between contexts.

---

## 2. Clean Architecture

Each service follows **Clean Architecture** with four layers:

```
┌─────────────────────────────────────────────────────────────┐
│  API Layer          │ Controllers, DTOs, Middleware, Swagger │
├─────────────────────────────────────────────────────────────┤
│  Application Layer  │ Commands, Queries, MediatR, Validators │
├─────────────────────────────────────────────────────────────┤
│  Domain Layer       │ Aggregates, Entities, Domain Events    │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure     │ EF Core, MassTransit, Repositories     │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Rule

- **Domain** has no dependencies on other layers
- **Application** depends only on Domain
- **Infrastructure** implements Application interfaces and depends on Domain
- **API** depends on Application and Infrastructure (composition root)

### Layer Responsibilities

| Layer | Responsibilities |
|-------|------------------|
| **API** | HTTP handling, request/response mapping, Swagger docs, middleware (correlation ID, exception handling) |
| **Application** | Use cases (CQRS), command/query handlers, validation (FluentValidation), MediatR pipeline behaviors |
| **Domain** | Business logic, aggregates, invariants, domain events |
| **Infrastructure** | EF Core DbContext, repositories, MassTransit, external API clients |

---

## 3. Event-Driven Design

### Message Flow

```
Transactions Service                    RabbitMQ                    Payments Service
        │                                    │                              │
        │  TransactionSubmitted              │                              │
        │ ─────────────────────────────────>│ ──────────────────────────> │
        │  (Transactional Outbox)            │  (queue: transaction-submitted)│
        │                                    │                              │
        │                                    │  PaymentConfirmed / PaymentFailed
        │ <─────────────────────────────────│ <────────────────────────── │
        │  (consumers)                       │                              │
```

### Reliability Patterns

| Pattern | Implementation |
|---------|----------------|
| **Transactional Outbox** | Domain events published before `SaveChanges`; MassTransit EF Outbox stores messages in DB; background process delivers to RabbitMQ |
| **Idempotent Consumers** | MassTransit EF Inbox deduplicates by `MessageId`; Payments also checks `GetByTransactionIdAsync` before creating |
| **Retry & Dead-Letter** | MassTransit default retry policy; failed messages move to error queue |

### Shared Contracts

Event contracts live in `MoneyFellows.Contracts`:

- `TransactionSubmitted`
- `PaymentConfirmed`
- `PaymentFailed`

---

## 4. CQRS Pattern

Commands and queries are separated via **MediatR**:

| Type | Purpose | Example |
|------|---------|---------|
| **Command** | State change, no return (or ID) | `CreateTransactionCommand`, `SubmitTransactionCommand` |
| **Query** | Read-only, returns DTO | `GetTransactionQuery`, `GetPaymentQuery` |

Validation runs in the **Application** layer via `ValidationBehavior` (MediatR pipeline) before handlers execute.

---

## 5. Technology Stack

| Concern | Technology |
|---------|------------|
| Runtime | .NET 8 |
| API | ASP.NET Core, API Versioning |
| CQRS | MediatR |
| Validation | FluentValidation |
| Persistence | EF Core 8, PostgreSQL |
| Messaging | MassTransit, RabbitMQ |
| Logging | Serilog, Seq |
| Testing | xUnit, Shouldly, WebApplicationFactory, Testcontainers |

---

## 6. Cross-Cutting Concerns

| Concern | Implementation |
|---------|----------------|
| **Correlation ID** | `CorrelationIdMiddleware` adds header; propagated to MassTransit messages |
| **Exception Handling** | `GlobalExceptionHandler` returns consistent JSON (400, 404, 409, 500) |
| **Health Checks** | `/health` endpoint; PostgreSQL + RabbitMQ checks |
| **API Documentation** | Swagger with request/response examples and error docs |

---

## 7. Data Flow Summary

### Transaction Lifecycle

1. **Create** → `POST /api/transactions` → `CreateTransactionCommand` → Transaction (Draft)
2. **Add Items** → `POST /api/transactions/{id}/items` → `AddTransactionItemCommand`
3. **Submit** → `POST /api/transactions/{id}/submit` → `SubmitTransactionCommand` → Publishes `TransactionSubmitted`
4. **Payment** → Payments consumes event (or `POST /api/payments/start`) → Creates Payment (Pending)
5. **Confirm/Fail** → `POST /api/payments/{id}/confirm` or `/fail` → Publishes `PaymentConfirmed` or `PaymentFailed`
6. **Complete** → Transactions consumes payment event → Transaction (Completed)

### Context Mapping

- **Transactions ↔ Payments**: Asynchronous (events only)
- **Payments → Transactions**: HTTP call to `GET /api/transactions/{id}` when starting payment (to fetch `TotalAmount`, `CustomerId`, `Status`)

---

## 8. Related Documentation

- **[DOMAIN.md](DOMAIN.md)** – Domain model, aggregates, invariants, events
- **[README.md](README.md)** – Setup, API reference, testing
- **[docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)** – Common issues and fixes
